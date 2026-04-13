using API.Extensions;
using Application.Authorization;
using Application.Constants;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Seeders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Reject request bodies larger than 10 MB at the Kestrel level before any
// middleware or controller logic runs, preventing memory exhaustion attacks.
builder.WebHost.ConfigureKestrel(o =>
    o.Limits.MaxRequestBodySize = 10 * 1024 * 1024);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddJwtAuthentication(builder.Environment)
    .AddCustomRateLimiting(builder.Configuration)
    .AddSwaggerDocumentation();

// Tenant-scoped user policy
builder.Services.AddSingleton<IAuthorizationHandler, UserOwnerOrAdminHandler>();

// System-owner policy (completely separate from tenant policies)
builder.Services.AddSingleton<IAuthorizationHandler, SystemOwnerOnlyHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(ClaimConstants.PolicyUserOwnerOrAdmin,
        policy => policy.AddRequirements(new UserOwnerOrAdminRequirement()));

    options.AddPolicy("SystemOwnerOnly",
        policy => policy.AddRequirements(new SystemOwnerOnlyRequirement()));
});

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    // Migrations are applied explicitly via APPLY_MIGRATIONS=true.
    // Never run automatically in production to avoid race conditions
    // when multiple instances start simultaneously.
    if (Environment.GetEnvironmentVariable("APPLY_MIGRATIONS") == "true")
    {
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }

    // Seed SystemOwner if none exists
    DatabaseSeeder seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync(app.Environment.IsProduction());
}

app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseSecurityHeaders();
app.UseHttpsRedirection();
app.UseCors(ClaimConstants.CorsPolicyName);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Public liveness probe — returns 200/503 with no dependency details.
// Safe to expose to load-balancer health checks and the internet.
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate      = _ => false,   // skip all checks; just confirm the process is up
    ResultStatusCodes =
    {
        [HealthStatus.Healthy]   = StatusCodes.Status200OK,
        [HealthStatus.Degraded]  = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
    },
});

// Detailed readiness probe — includes DB, Redis, and SMTP status.
// Restricted to callers that supply the correct HEALTH_API_KEY header so that
// internal infra (Kubernetes readiness probes, uptime monitors) can use it
// without exposing dependency topology to the public internet.
string healthApiKey = Environment.GetEnvironmentVariable("HEALTH_API_KEY") ?? string.Empty;

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    ResultStatusCodes =
    {
        [HealthStatus.Healthy]   = StatusCodes.Status200OK,
        [HealthStatus.Degraded]  = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
    },
}).AddEndpointFilter(async (ctx, next) =>
{
    // When no key is configured (e.g. local dev) allow all callers.
    if (string.IsNullOrWhiteSpace(healthApiKey))
        return await next(ctx);

    if (!ctx.HttpContext.Request.Headers.TryGetValue("X-Health-Api-Key", out var provided)
        || provided != healthApiKey)
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Results.Unauthorized();
    }

    return await next(ctx);
});

app.Run();

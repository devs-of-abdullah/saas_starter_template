using API.Extensions;
using Application.Authorization;
using Application.Constants;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Seeders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddJwtAuthentication()
    .AddCustomRateLimiting()
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
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (Environment.GetEnvironmentVariable("APPLY_MIGRATIONS") == "true" || app.Environment.IsDevelopment())
    {
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
app.MapHealthChecks("/health");

app.Run();

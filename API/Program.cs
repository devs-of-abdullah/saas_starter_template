using API.Extensions;
using Application.Authorization;
using Application.Constants;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration).AddJwtAuthentication().AddCustomRateLimiting().AddSwaggerDocumentation();

builder.Services.AddSingleton<IAuthorizationHandler, UserOwnerOrAdminHandler>();
builder.Services.AddAuthorization(options => { options.AddPolicy(ClaimConstants.PolicyUserOwnerOrAdmin, policy => policy.AddRequirements(new UserOwnerOrAdminRequirement())); });

WebApplication app = builder.Build();


using (IServiceScope scope = app.Services.CreateScope())
{
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (Environment.GetEnvironmentVariable("APPLY_MIGRATIONS") == "true" || app.Environment.IsDevelopment())
    {
        db.Database.Migrate();
    }
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

using Domain.Enums.User;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Infrastructure.Constants;
using Application.Interfaces.Services.User;

namespace Infrastructure.Services.User;

public sealed class CurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; }
    public Guid? TenantId { get; }
    public bool IsSystemOwner { get; }
    public bool IsAuthenticated { get; }


    public CurrentUserService(IHttpContextAccessor accessor, ILogger<CurrentUserService> logger)
    {
        var user = accessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
            return;

        IsAuthenticated = true;
        UserId = ParseGuidClaim(user, ClaimTypes.NameIdentifier, logger);
        TenantId = ParseGuidClaim(user, ClaimNames.TenantId, logger);
        IsSystemOwner = user.IsInRole(UserRoles.SystemOwner);

        IsSystemOwner = user.IsInRole(UserRoles.SystemOwner);
    }
    static Guid? ParseGuidClaim(ClaimsPrincipal user, string claimType, ILogger logger)
    {
        var value = user.FindFirst(claimType)?.Value;

        if (value is null)
            return null;

        if(Guid.TryParse(value, out var guid))
            return guid;

        logger.LogWarning("Failed to parse claim {ClaimType} with value {ClaimValue} as Guid", claimType, value);
        return null;

    }
}
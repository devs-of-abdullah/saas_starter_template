using Application.Constants;
using Application.Interfaces.Services.User;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Infrastructure.Services.User;

/// <summary>Extracts identity claims from the current HTTP request's JWT principal.</summary>
public sealed class CurrentUserService : ICurrentUserService
{
    /// <inheritdoc/>
    public Guid? UserId { get; }

    /// <inheritdoc/>
    public Guid? TenantId { get; }

    /// <inheritdoc/>
    public bool IsSystemOwner { get; }

    /// <inheritdoc/>
    public bool IsAuthenticated { get; }

    /// <summary>Initialises the service from the current HTTP context.</summary>
    public CurrentUserService(IHttpContextAccessor accessor, ILogger<CurrentUserService> logger)
    {
        ClaimsPrincipal? user = accessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
            return;

        IsAuthenticated = true;
        UserId = ParseGuidClaim(user, ClaimTypes.NameIdentifier, logger);
        TenantId = ParseGuidClaim(user, ClaimConstants.TenantId, logger);
        IsSystemOwner = user.IsInRole("SystemOwner");
    }

    private static Guid? ParseGuidClaim(ClaimsPrincipal user, string claimType, ILogger logger)
    {
        string? value = user.FindFirst(claimType)?.Value;

        if (value is null)
            return null;

        if (Guid.TryParse(value, out Guid guid))
            return guid;

        logger.LogWarning(
            "Failed to parse claim {ClaimType} with value {ClaimValue} as Guid",
            claimType, value);
        return null;
    }
}

using Domain.Enums.User;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Application.Authorization;

public sealed class UserOwnerOrAdminHandler : AuthorizationHandler<UserOwnerOrAdminRequirement, Guid>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,UserOwnerOrAdminRequirement requirement, Guid requestedUserId)
    {
        if (context.User.IsInRole(UserRole.TenantSuperAdmin.ToString()) || context.User.IsInRole(UserRole.TenantAdmin.ToString()))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        string? claim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(claim, out Guid authenticatedUserId) && authenticatedUserId == requestedUserId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

using Microsoft.AspNetCore.Authorization;

namespace Application.Authorization;

public sealed class SystemOwnerOnlyHandler : AuthorizationHandler<SystemOwnerOnlyRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SystemOwnerOnlyRequirement requirement)
    {
        if (context.User.IsInRole("SystemOwner"))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

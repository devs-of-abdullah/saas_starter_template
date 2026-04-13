using Microsoft.AspNetCore.Authorization;

namespace Application.Authorization;

public sealed class SystemOwnerOnlyRequirement : IAuthorizationRequirement { }

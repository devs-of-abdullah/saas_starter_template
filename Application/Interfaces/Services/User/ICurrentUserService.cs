namespace Application.Interfaces.Services.User;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    bool IsSystemOwner { get; }
    bool IsAuthenticated { get; }

}
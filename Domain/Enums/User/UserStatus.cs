namespace Domain.Enums.User;

public enum UserStatus
{
    PendingVerification = 0,
    Active = 1,
    Suspended = 2,
    Banned = 3,
}
public static class UserStatuses
{
    public const string PendingVerification = nameof(UserStatus.PendingVerification);
    public const string Active = nameof(UserStatus.Active);
    public const string Suspended = nameof(UserStatus.Suspended);
    public const string Banned = nameof(UserStatus.Banned);
}
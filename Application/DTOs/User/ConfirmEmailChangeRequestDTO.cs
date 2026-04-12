namespace Application.DTOs.User;

public sealed record ConfirmEmailChangeRequestDTO
{
    public string Code { get; init; } = null!;
}

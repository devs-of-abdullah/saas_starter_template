namespace API.Models;

public sealed record ApiResponse<T>
{
    public int StatusCode { get; init; }

    public string Message { get; init; } = null!;

    public object? Errors { get; init; }

    public T? Data { get; init; }

    public static ApiResponse<T> Ok(string message, T? data = default) => new()
    {
        StatusCode = StatusCodes.Status200OK,
        Message = message,
        Data = data
    };

    public static ApiResponse<T> Created(string message, T? data = default) => new()
    {
        StatusCode = StatusCodes.Status201Created,
        Message = message,
        Data = data
    };
}

public sealed record ApiResponse
{
    public int StatusCode { get; init; }

    public string Message { get; init; } = null!;
    public object? Errors { get; init; }

    public static ApiResponse Ok(string message) => new()
    {
        StatusCode = StatusCodes.Status200OK,
        Message = message
    };

    public static ApiResponse Error(int statusCode, string message, object? errors = null) => new()
    {
        StatusCode = statusCode,
        Message = message,
        Errors = errors
    };
}

using API.Models;
using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using DomainValidationException = Domain.Exceptions.ValidationException;

namespace API.Extensions;

public static class ExceptionExtensions
{
    public static void UseGlobalExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                Exception? exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                (int statusCode, string message) = exception switch
                {
                    NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                    ConflictException => (StatusCodes.Status409Conflict, exception.Message),
                    UnauthorizedException => (StatusCodes.Status401Unauthorized, exception.Message),
                    ForbiddenException => (StatusCodes.Status403Forbidden, exception.Message),
                    TooManyRequestsException => (StatusCodes.Status429TooManyRequests, exception.Message),
                    DomainValidationException => (StatusCodes.Status422UnprocessableEntity, exception.Message),
                    _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
                };

                object? errors = exception is DomainValidationException valEx ? valEx.Errors : null;

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(
                    ApiResponse.Error(statusCode, message, errors));
            });
        });
    }
}

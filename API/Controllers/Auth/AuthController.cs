using API.Models;
using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers.Auth;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[ApiController]
[EnableRateLimiting("AuthLimiter")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request, CancellationToken ct)
    {
        await _authService.RegisterAsync(request, ct);
        return Ok(ApiResponse.Ok("Registration successful. Please check your email for a verification code."));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request, CancellationToken ct)
    {
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        string userAgent = Request.Headers.UserAgent.ToString();
        string? deviceInfo = Request.Headers["X-Device-Info"].FirstOrDefault();

        TokenResponseDTO result = await _authService.LoginAsync(request, ipAddress, userAgent, deviceInfo, ct);
        return Ok(new ApiResponse<TokenResponseDTO>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Login successful.",
            Data = result
        });
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshRequestDTO request, CancellationToken ct)
    {
        TokenResponseDTO result = await _authService.RefreshAsync(request, ct);
        return Ok(new ApiResponse<TokenResponseDTO>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Token refreshed.",
            Data = result
        });
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDTO request, CancellationToken ct)
    {
        await _authService.LogoutAsync(request.RefreshToken, ct);
        return NoContent();
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDTO request, CancellationToken ct)
    {
        EmailVerificationResult result = await _authService.VerifyEmailAsync(request, ct);

        return result switch
        {
            EmailVerificationResult.Success => Ok(ApiResponse.Ok("Email verified successfully.")), EmailVerificationResult.AlreadyVerified => Ok(ApiResponse.Ok("Email is already verified.")),
            EmailVerificationResult.Expired => BadRequest(ApiResponse.Error(StatusCodes.Status400BadRequest, "Verification code has expired. Please request a new one.")), _ => BadRequest(ApiResponse.Error(StatusCodes.Status400BadRequest, "Invalid verification code."))
        };
    }

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequestDTO request, CancellationToken ct)
    {
        await _authService.ResendVerificationEmailAsync(request, ct);
        return Ok(ApiResponse.Ok("If the account exists and is unverified, a new code has been sent."));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request, CancellationToken ct)
    {
        await _authService.ForgotPasswordAsync(request, ct);
        return Ok(ApiResponse.Ok("If the account exists, a reset code has been sent."));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request, CancellationToken ct)
    {
        bool success = await _authService.ResetPasswordAsync(request, ct);
        if (!success)
            return BadRequest(ApiResponse.Error(StatusCodes.Status400BadRequest, "Invalid or expired reset code."));

        return Ok(ApiResponse.Ok("Password reset successfully. Please log in with your new password."));
    }
}

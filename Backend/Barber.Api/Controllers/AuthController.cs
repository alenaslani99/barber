using Barber.Api.DTO;
using Barber.Application.Auth;
using Barber.Application.UseCases;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Barber.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class AuthController(
    RegisterUserHandler register,
    LoginUserHandler login,
    RefreshSessionHandler refresh,
    LogoutUserHandler logout,
    IValidator<RegisterUserCommand> registerValidator,
    IValidator<LoginUserCommand> loginValidator,
    IWebHostEnvironment env,
    IOptions<JwtSettings> jwt) : ControllerBase
{
    private const string RefreshCookieName = "barber_refresh";

    [HttpPost("register")]
    public async Task<ActionResult<SessionResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        RegisterUserCommand command = new(request.Email, request.Password, request.FirstName, request.LastName, request.Phone);
        await registerValidator.ValidateAndThrowAsync(command, ct);
        AuthResult result = await register.HandleAsync(command, ct);
        SetRefreshCookie(result.RefreshToken);
        return Ok(ToResponse(result));
    }

    [HttpPost("login")]
    public async Task<ActionResult<SessionResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        LoginUserCommand command = new(request.Email, request.Password);
        await loginValidator.ValidateAndThrowAsync(command, ct);
        AuthResult result = await login.HandleAsync(command, ct);
        SetRefreshCookie(result.RefreshToken);
        return Ok(ToResponse(result));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<SessionResponse>> Refresh(CancellationToken ct)
    {
        if (!Request.Cookies.TryGetValue(RefreshCookieName, out string? refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized();
        AuthResult result = await refresh.HandleAsync(new RefreshSessionCommand(refreshToken), ct);
        SetRefreshCookie(result.RefreshToken);
        return Ok(ToResponse(result));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (Request.Cookies.TryGetValue(RefreshCookieName, out string? refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
            await logout.HandleAsync(new LogoutUserCommand(refreshToken), ct);
        Response.Cookies.Delete(RefreshCookieName, DeleteCookieOptions());
        return NoContent();
    }

    private static SessionResponse ToResponse(AuthResult result) =>
        new(result.AccessToken, result.AccessTokenExpiresAt);

    private void SetRefreshCookie(string refreshToken)
    {
        Response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(jwt.Value.RefreshTokenDays)
        });
    }

    private CookieOptions DeleteCookieOptions() => new()
    {
        Path = "/api/auth",
        SameSite = SameSiteMode.Lax,
        Secure = !env.IsDevelopment()
    };
}

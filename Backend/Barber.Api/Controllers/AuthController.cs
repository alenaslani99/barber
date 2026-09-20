using Barber.Api.DTO;
using Barber.Application.Auth;
using Barber.Application.UseCases;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    IValidator<LoginUserCommand> loginValidator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        RegisterUserCommand command = new(request.Email, request.Password, request.FirstName, request.LastName, request.Phone);
        await registerValidator.ValidateAndThrowAsync(command, ct);
        AuthResult result = await register.HandleAsync(command, ct);
        return Ok(ToResponse(result));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        LoginUserCommand command = new(request.Email, request.Password);
        await loginValidator.ValidateAndThrowAsync(command, ct);
        AuthResult result = await login.HandleAsync(command, ct);
        return Ok(ToResponse(result));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken ct)
    {
        AuthResult result = await refresh.HandleAsync(new RefreshSessionCommand(request.RefreshToken), ct);
        return Ok(ToResponse(result));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest request, CancellationToken ct)
    {
        await logout.HandleAsync(new LogoutUserCommand(request.RefreshToken), ct);
        return NoContent();
    }

    private static AuthResponse ToResponse(AuthResult result) =>
        new(result.AccessToken, result.AccessTokenExpiresAt, result.RefreshToken);
}

namespace Barber.Api.DTO;

public sealed record RegisterRequest(string Email, string Password, string FirstName, string LastName, string Phone);
public sealed record LoginRequest(string Email, string Password);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public sealed record SessionResponse(string AccessToken, DateTimeOffset AccessTokenExpiresAt);

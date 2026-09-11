using Barber.Application.Auth;
using Barber.Domain;
using Microsoft.AspNetCore.Identity;

namespace Barber.Api.Core;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(User user, string password) =>
        _hasher.HashPassword(user, password);

    public bool VerifyPassword(User user, string passwordHash, string password) =>
        _hasher.VerifyHashedPassword(user, passwordHash, password)
            is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
}

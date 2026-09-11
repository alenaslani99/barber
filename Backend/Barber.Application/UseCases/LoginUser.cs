using Barber.Application.Auth;
using Barber.Application.Exceptions;
using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Barber.Application.UseCases;

public sealed record LoginUserCommand(string Email, string Password);

public sealed class LoginUserHandler(
    TenantContext db,
    IPasswordService passwords,
    IJwtTokenService tokens,
    IOptions<JwtSettings> settings,
    ITenantProvider tenants)
{
    public async Task<AuthResult> HandleAsync(LoginUserCommand command, CancellationToken ct = default)
    {
        User? user = await db.Users.FirstOrDefaultAsync(u => u.Email == command.Email, ct);
        if (user is null || !user.IsActive || !passwords.VerifyPassword(user, user.PasswordHash, command.Password))
            throw new InvalidCredentialsException();

        return await SessionIssuer.IssueAsync(db, tokens, settings.Value, user, tenants.GetSlug(), ct);
    }
}

using Barber.Application.Auth;
using Barber.Application.Exceptions;
using Barber.Application.Logging;
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
    ITenantProvider tenants,
    AuditWriter audit)
{
    public async Task<AuthResult> HandleAsync(LoginUserCommand command, CancellationToken ct = default)
    {
        User? user = await db.Users.FirstOrDefaultAsync(u => u.Email == command.Email, ct);
        if (user is null || !user.IsActive || !passwords.VerifyPassword(user, user.PasswordHash, command.Password))
        {
            await audit.WriteAsync("auth.login_failed", command.Email, null, false, "invalid_credentials", ct: ct);
            throw new InvalidCredentialsException();
        }

        AuthResult result = await SessionIssuer.IssueAsync(db, tokens, settings.Value, user, tenants.GetSlug(), ct);
        await audit.WriteAsync("auth.login", user.Email, user.Id, ct: ct);
        return result;
    }
}

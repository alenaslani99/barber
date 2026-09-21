using Barber.Application.Auth;
using Barber.Application.Exceptions;
using Barber.Application.Logging;
using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Barber.Application.UseCases;

public sealed record RegisterUserCommand(string Email, string Password, string FirstName, string LastName, string Phone);

public sealed class RegisterUserHandler(
    TenantContext db,
    IPasswordService passwords,
    IJwtTokenService tokens,
    IOptions<JwtSettings> settings,
    ITenantProvider tenants,
    AuditWriter audit)
{
    public async Task<AuthResult> HandleAsync(RegisterUserCommand command, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(u => u.Email == command.Email, ct))
            throw new DuplicateEmailException(command.Email);

        if (await db.Users.AnyAsync(u => u.Phone == command.Phone, ct))
            throw new DuplicatePhoneException(command.Phone);

        User user = new()
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            Phone = command.Phone,
            PasswordHash = string.Empty,
            Role = UserRole.Client
        };
        user.PasswordHash = passwords.HashPassword(user, command.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        AuthResult result = await SessionIssuer.IssueAsync(db, tokens, settings.Value, user, tenants.GetSlug(), ct);
        await audit.WriteAsync("auth.register", user.Email, user.Id, ct: ct);
        return result;
    }
}

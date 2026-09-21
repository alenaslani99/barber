using Barber.Application.Auth;
using Barber.Application.Exceptions;
using Barber.Application.Logging;
using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword);

public sealed class ChangePasswordHandler(TenantContext db, IPasswordService passwords, AuditWriter audit)
{
    public async Task HandleAsync(ChangePasswordCommand command, CancellationToken ct = default)
    {
        User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, ct);
        if (user is null || !user.IsActive || !passwords.VerifyPassword(user, user.PasswordHash, command.CurrentPassword))
        {
            await audit.WriteAsync("auth.password_change_failed", user?.Email ?? string.Empty, command.UserId, false, "invalid_current_password", ct: ct);
            throw new InvalidCredentialsException();
        }

        user.PasswordHash = passwords.HashPassword(user, command.NewPassword);
        await db.SaveChangesAsync(ct);
        await audit.WriteAsync("auth.password_changed", user.Email, user.Id, ct: ct);
    }
}

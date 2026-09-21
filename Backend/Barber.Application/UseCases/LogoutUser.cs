using Barber.Application.Logging;
using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record LogoutUserCommand(string RefreshToken);

public sealed class LogoutUserHandler(TenantContext db, AuditWriter audit)
{
    public async Task HandleAsync(LogoutUserCommand command, CancellationToken ct = default)
    {
        RefreshToken? stored = await SessionIssuer.FindActiveAsync(db, command.RefreshToken, ct);
        if (stored is null)
            return;

        stored.RevokedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        User? user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == stored.UserId, ct);
        await audit.WriteAsync("auth.logout", user?.Email ?? string.Empty, stored.UserId, ct: ct);
    }
}

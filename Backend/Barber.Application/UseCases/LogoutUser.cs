using Barber.DataAccess;
using Barber.Domain;

namespace Barber.Application.UseCases;

public sealed record LogoutUserCommand(string RefreshToken);

public sealed class LogoutUserHandler(TenantContext db)
{
    public async Task HandleAsync(LogoutUserCommand command, CancellationToken ct = default)
    {
        RefreshToken? stored = await SessionIssuer.FindActiveAsync(db, command.RefreshToken, ct);
        if (stored is null)
            return;

        stored.RevokedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}

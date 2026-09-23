using Barber.Application.Auth;
using Barber.Application.Exceptions;
using Barber.Application.Logging;
using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Barber.Application.UseCases;

public sealed record RefreshSessionCommand(string RefreshToken);

public sealed class RefreshSessionHandler(
    TenantContext db,
    IJwtTokenService tokens,
    IOptions<JwtSettings> settings,
    ITenantProvider tenants,
    AuditWriter audit)
{
    public async Task<AuthResult> HandleAsync(RefreshSessionCommand command, CancellationToken ct = default)
    {
        string hash = SessionIssuer.Hash(command.RefreshToken);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        RefreshToken? stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == hash, ct);
        if (stored is null || stored.ExpiresAt <= now)
            throw new InvalidCredentialsException();

        User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == stored.UserId, ct);
        if (user is null || !user.IsActive)
            throw new InvalidCredentialsException();

        if (stored.RevokedAt is not null)
        {
            List<RefreshToken> family = await db.RefreshTokens
                .Where(t => t.UserId == stored.UserId && t.RevokedAt == null)
                .ToListAsync(ct);
            foreach (RefreshToken token in family)
                token.RevokedAt = now;
            await db.SaveChangesAsync(ct);
            await audit.WriteAsync("auth.refresh_reuse", user.Email, user.Id, false, "token_reuse", ct: ct);
            throw new InvalidCredentialsException();
        }

        stored.RevokedAt = now;
        await db.SaveChangesAsync(ct);
        stored.RevokedAt = now;
        AuthResult result = await SessionIssuer.IssueAsync(db, tokens, settings.Value, user, tenants.GetSlug(), ct);
        return result;
    }
}

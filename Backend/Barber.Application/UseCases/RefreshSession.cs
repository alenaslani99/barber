using Barber.Application.Auth;
using Barber.Application.Exceptions;
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
    ITenantProvider tenants)
{
    public async Task<AuthResult> HandleAsync(RefreshSessionCommand command, CancellationToken ct = default)
    {
        RefreshToken? stored = await SessionIssuer.FindActiveAsync(db, command.RefreshToken, ct);
        if (stored is null)
            throw new InvalidCredentialsException();

        stored.RevokedAt = DateTimeOffset.UtcNow;
        User user = await db.Users.FirstAsync(u => u.Id == stored.UserId, ct);
        if (!user.IsActive)
            throw new InvalidCredentialsException();

        return await SessionIssuer.IssueAsync(db, tokens, settings.Value, user, tenants.GetSlug(), ct);
    }
}

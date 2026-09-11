using Barber.Application.Auth;
using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Barber.Application.UseCases;

internal static class SessionIssuer
{
    public static async Task<AuthResult> IssueAsync(
        TenantContext db,
        IJwtTokenService tokens,
        JwtSettings settings,
        User user,
        string tenantSlug,
        CancellationToken ct)
    {
        (string AccessToken, DateTimeOffset ExpiresAt) access = tokens.CreateAccessToken(user, tenantSlug);
        string refreshRaw = tokens.CreateRefreshToken();
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = Hash(refreshRaw),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(settings.RefreshTokenDays)
        });
        await db.SaveChangesAsync(ct);
        return new AuthResult(access.AccessToken, access.ExpiresAt, refreshRaw);
    }

    public static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public static async Task<RefreshToken?> FindActiveAsync(
        TenantContext db, string refreshToken, CancellationToken ct)
    {
        string hash = Hash(refreshToken);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return await db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == hash && t.RevokedAt == null && t.ExpiresAt > now, ct);
    }
}

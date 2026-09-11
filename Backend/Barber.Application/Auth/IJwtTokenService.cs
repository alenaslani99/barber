using Barber.Domain;

namespace Barber.Application.Auth;

public interface IJwtTokenService
{
    (string AccessToken, DateTimeOffset ExpiresAt) CreateAccessToken(User user, string tenantSlug);
    string CreateRefreshToken();
}

namespace Barber.Domain;

public sealed class RefreshToken : Entity
{
    public Guid UserId { get; set; }
    public required string Token { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RevokedAt { get; set; }
}

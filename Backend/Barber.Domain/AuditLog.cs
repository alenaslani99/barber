namespace Barber.Domain;

public sealed class AuditLog : Entity
{
    public Guid? UserId { get; set; }
    public required string Email { get; set; }
    public required string Action { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public bool Success { get; set; } = true;
    public string? FailureReason { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

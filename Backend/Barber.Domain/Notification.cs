namespace Barber.Domain;

public sealed class Notification : Entity
{
    public Guid BarbershopId { get; set; }
    public NotificationChannel Channel { get; set; }
    public required string Recipient { get; set; }
    public string? Subject { get; set; }
    public required string Body { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public DateTimeOffset? ScheduledAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

namespace Barber.Domain;

public sealed class Booking : Entity
{
    public Guid BarbershopId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid StaffId { get; set; }
    public Guid ClientId { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public string? Notes { get; set; }
}

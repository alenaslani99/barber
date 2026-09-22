namespace Barber.Domain;

public sealed class WorkingHours : Entity
{
    public Guid BarbershopId { get; set; }
    public DayOfWeek Day { get; set; }
    public TimeOnly Open { get; set; }
    public TimeOnly Close { get; set; }
    public bool IsClosed { get; set; }
}

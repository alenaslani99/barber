namespace Barber.Domain;

public sealed class WorkingDayOverride : Entity
{
    public Guid BarbershopId { get; set; }
    public Guid? StaffId { get; set; }
    public DateOnly Date { get; set; }
    public string? Reason { get; set; }
}

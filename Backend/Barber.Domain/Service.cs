namespace Barber.Domain;

public sealed class Service : NamedEntity
{
    public Guid BarbershopId { get; set; }
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public int SlotMinutes { get; set; } = 30;
    public bool IsActive { get; set; } = true;
}

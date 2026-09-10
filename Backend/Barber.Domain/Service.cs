namespace Barber.Domain;

public sealed class Service : NamedEntity
{
    public Guid BarbershopId { get; set; }
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsActive { get; set; } = true;
}

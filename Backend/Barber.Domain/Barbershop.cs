namespace Barber.Domain;

public sealed class Barbershop : NamedEntity
{
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string TimeZone { get; set; } = "Europe/Berlin";
}

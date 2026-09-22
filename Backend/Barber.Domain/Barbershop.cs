namespace Barber.Domain;

public sealed class Barbershop : NamedEntity
{
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string Tagline { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "Europe/Berlin";
}

namespace Barber.Domain;

public sealed class Tenant : Entity
{
    public required string Slug { get; set; }
    public required string DatabaseName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

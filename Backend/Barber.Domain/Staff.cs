namespace Barber.Domain;

public sealed class Staff : Entity
{
    public Guid BarbershopId { get; set; }
    public Guid? UserId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public bool IsActive { get; set; } = true;
}

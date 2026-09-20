namespace Barber.Domain;

public sealed class Staff : Entity
{
    public Guid BarbershopId { get; set; }
    public Guid UserId { get; set; }
    public Guid SeniorityId { get; set; }
    public bool IsActive { get; set; } = true;
}

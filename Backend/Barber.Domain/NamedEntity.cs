namespace Barber.Domain;

public abstract class NamedEntity : Entity
{
    public required string Name { get; set; }
}

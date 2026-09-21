namespace Barber.Api.DTO;

public sealed record BarberResponse(Guid Id, string FirstName, string LastName, string Seniority);
public sealed record ServiceResponse(Guid Id, string Name, int DurationMinutes, decimal Price);

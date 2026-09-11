namespace Barber.Api.DTO;

public sealed record CreateStaffRequest(string Email, Guid BarbershopId);
public sealed record StaffResponse(Guid Id, Guid? UserId, string FirstName, string LastName);

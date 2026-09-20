namespace Barber.Api.DTO;

public sealed record CreateStaffRequest(string Email, Guid BarbershopId, Guid? SeniorityId);
public sealed record StaffResponse(Guid Id, Guid UserId, Guid BarbershopId, Guid SeniorityId, bool IsActive);

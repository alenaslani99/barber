namespace Barber.Api.DTO;

public sealed record ProfileResponse(Guid Id, string FirstName, string LastName, string Email, string Phone);
public sealed record MyBookingResponse(Guid Id, string ServiceName, string BarberName, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string Status);
public sealed record MyBookingsResponse(List<MyBookingResponse> Items, int Total);
public sealed record CreateBookingRequest(Guid ServiceId, Guid StaffId, DateTimeOffset StartsAt, string? Notes);
public sealed record BookingResponse(Guid Id, string Status, DateTimeOffset StartsAt, DateTimeOffset EndsAt);
public sealed record AdminBookingResponse(Guid Id, string ServiceName, string BarberName, string ClientName, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string Status);
public sealed record AdminBookingsResponse(List<AdminBookingResponse> Items, int Total);

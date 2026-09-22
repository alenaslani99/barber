namespace Barber.Api.DTO;

public sealed record BarberResponse(Guid Id, string FirstName, string LastName, string Seniority, bool IsActive);
public sealed record ServiceResponse(Guid Id, string Name, int DurationMinutes, decimal Price);
public sealed record ServiceDetailResponse(Guid Id, string Name, decimal Price, int DurationMinutes, int SlotMinutes, bool IsActive);
public sealed record CreateServiceRequest(Guid BarbershopId, string Name, decimal Price, int DurationMinutes, int SlotMinutes);
public sealed record UpdateServiceRequest(string? Name, decimal? Price, int? DurationMinutes, int? SlotMinutes, bool? IsActive);
public sealed record UpdateStaffRequest(bool IsActive);
public sealed record WorkingHoursResponse(DayOfWeek Day, string Open, string Close, bool IsClosed);
public sealed record WorkingHoursInputRequest(DayOfWeek Day, string Open, string Close, bool IsClosed);
public sealed record UpdateWorkingHoursRequest(Guid BarbershopId, List<WorkingHoursInputRequest> Hours);
public sealed record ShopHourResponse(int Day, string Open, string Close, bool Closed);
public sealed record ShopResponse(
    Guid Id,
    string Name,
    string Tagline,
    string Description,
    string? Address,
    string? Phone,
    List<ShopHourResponse> Hours,
    List<BarberResponse> Staff,
    List<ServiceResponse> Services);

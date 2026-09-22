namespace Barber.Api.DTO;

public sealed record BarberResponse(Guid Id, string FirstName, string LastName, string Seniority);
public sealed record ServiceResponse(Guid Id, string Name, int DurationMinutes, decimal Price);
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

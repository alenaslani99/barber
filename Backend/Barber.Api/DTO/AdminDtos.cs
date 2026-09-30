namespace Barber.Api.DTO;

public sealed record CreateTenantRequest(string Slug, string DatabaseName);
public sealed record TenantResponse(Guid Id, string Slug, string DatabaseName, bool IsActive, DateTimeOffset CreatedAt);
public sealed record UpsertShopRequest(
    string Name,
    string? Address,
    string? Phone,
    string? Tagline,
    string? Description,
    string? TimeZone);
public sealed record ShopDetailsResponse(
    Guid Id,
    string Name,
    string? Address,
    string? Phone,
    string Tagline,
    string Description,
    string TimeZone);
public sealed record CreateOwnerRequest(string FirstName, string LastName, string Email, string Phone);
public sealed record OwnerResponse(Guid Id, string FirstName, string LastName, string Email, string Phone);
public sealed record CredentialsResponse(Guid UserId, string FirstName, string LastName, string Email, string Password);
public sealed record TenantSetupResponse(
    TenantResponse Tenant,
    bool HasShop,
    bool HasOwner,
    int StaffCount,
    int ServiceCount);

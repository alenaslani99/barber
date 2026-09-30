namespace Barber.Api.DTO;

public sealed record CreateTenantRequest(string Slug, string DatabaseName);
public sealed record TenantResponse(Guid Id, string Slug, string DatabaseName, bool IsActive, DateTimeOffset CreatedAt);
public sealed record TenantSetupResponse(
    TenantResponse Tenant,
    bool HasShop,
    bool HasOwner,
    int StaffCount,
    int ServiceCount);

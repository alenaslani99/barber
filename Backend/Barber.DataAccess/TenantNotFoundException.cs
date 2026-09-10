namespace Barber.DataAccess;

public sealed class TenantNotFoundException(string slug)
    : Exception($"Unknown or inactive tenant '{slug}'.");

using Microsoft.EntityFrameworkCore;

namespace Barber.DataAccess;

/// <summary>
/// Opens a <see cref="TenantContext"/> for an explicit database name. Used by admin
/// provisioning, where the target database is not the one resolved from the request.
/// </summary>
public sealed class TenantDatabases(string template)
{
    public TenantContext Open(string databaseName)
    {
        DbContextOptions<TenantContext> options = new DbContextOptionsBuilder<TenantContext>()
            .UseNpgsql($"{template};Database={databaseName}")
            .Options;
        return new TenantContext(options);
    }
}

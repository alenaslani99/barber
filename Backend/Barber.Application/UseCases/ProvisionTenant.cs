using Barber.Application.Exceptions;
using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Barber.Application.UseCases;

public sealed record ProvisionTenantCommand(string Slug, string DatabaseName);
public sealed record TenantResult(Guid Id, string Slug, string DatabaseName, bool IsActive, DateTimeOffset CreatedAt);

/// <summary>
/// Creates the tenant database, applies every tenant migration, then registers the
/// catalog row. If anything fails after the database exists, it is dropped again.
/// </summary>
public sealed class ProvisionTenantHandler(
    CatalogContext catalog,
    TenantDatabases databases,
    ILogger<ProvisionTenantHandler> logger)
{
    public async Task<TenantResult> HandleAsync(ProvisionTenantCommand command, CancellationToken ct = default)
    {
        if (await catalog.Tenants.AnyAsync(t => t.Slug == command.Slug, ct))
            throw new DuplicateTenantException($"Slug '{command.Slug}' is already taken.");

        if (await catalog.Tenants.AnyAsync(t => t.DatabaseName == command.DatabaseName, ct))
            throw new DuplicateTenantException($"Database '{command.DatabaseName}' is already used by another tenant.");

        // Raw SQL: pg_database is a Postgres system catalog with no EF model. Guards against
        // migrating into (and later dropping) a database that exists outside the catalog.
        bool databaseExists = await catalog.Database
            .SqlQuery<int>($"SELECT 1 AS \"Value\" FROM pg_database WHERE datname = {command.DatabaseName}")
            .AnyAsync(ct);
        if (databaseExists)
            throw new DuplicateTenantException($"Database '{command.DatabaseName}' already exists on the server.");

        await using TenantContext tenantDb = databases.Open(command.DatabaseName);
        try
        {
            // Npgsql creates the database when it does not exist yet.
            await tenantDb.Database.MigrateAsync(ct);

            Tenant tenant = new()
            {
                Slug = command.Slug,
                DatabaseName = command.DatabaseName,
                IsActive = true
            };
            catalog.Tenants.Add(tenant);
            await catalog.SaveChangesAsync(ct);

            return new TenantResult(tenant.Id, tenant.Slug, tenant.DatabaseName, tenant.IsActive, tenant.CreatedAt);
        }
        catch
        {
            await DropAsync(tenantDb, command.DatabaseName);
            throw;
        }
    }

    private async Task DropAsync(TenantContext tenantDb, string databaseName)
    {
        try
        {
            await tenantDb.Database.EnsureDeletedAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            // Never mask the original failure; the leftover database needs a manual drop.
            logger.LogError(ex, "Failed to drop tenant database {DatabaseName} after provisioning error.", databaseName);
        }
    }
}

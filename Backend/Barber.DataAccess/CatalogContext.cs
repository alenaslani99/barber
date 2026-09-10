using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.DataAccess;

public sealed class CatalogContext : DbContext
{
    public CatalogContext(DbContextOptions<CatalogContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogContext).Assembly);
        modelBuilder.Ignore<Barbershop>();
        modelBuilder.Ignore<Service>();
        modelBuilder.Ignore<Staff>();
        modelBuilder.Ignore<Client>();
        modelBuilder.Ignore<Booking>();
        modelBuilder.Ignore<User>();
        modelBuilder.Ignore<RefreshToken>();
        modelBuilder.Ignore<Notification>();
        base.OnModelCreating(modelBuilder);
    }
}

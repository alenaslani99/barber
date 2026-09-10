using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.DataAccess;

public sealed class TenantContext : DbContext
{
    public TenantContext(DbContextOptions<TenantContext> options) : base(options)
    {
    }

    public DbSet<Barbershop> Barbershops => Set<Barbershop>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantContext).Assembly);
        modelBuilder.Ignore<Tenant>();
        base.OnModelCreating(modelBuilder);
    }
}

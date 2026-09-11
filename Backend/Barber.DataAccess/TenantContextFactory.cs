using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Barber.DataAccess;

public sealed class TenantContextFactory : IDesignTimeDbContextFactory<TenantContext>
{
    public TenantContext CreateDbContext(string[] args)
    {
        DbContextOptions<TenantContext> options = new DbContextOptionsBuilder<TenantContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=barber_t_design;Username=postgres;Password=postgres")
            .Options;
        return new TenantContext(options);
    }
}

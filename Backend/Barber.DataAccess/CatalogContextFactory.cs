using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Barber.DataAccess;

public sealed class CatalogContextFactory : IDesignTimeDbContextFactory<CatalogContext>
{
    public CatalogContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CatalogContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=barber_catalog;Username=postgres;Password=postgres")
            .Options;
        return new CatalogContext(options);
    }
}

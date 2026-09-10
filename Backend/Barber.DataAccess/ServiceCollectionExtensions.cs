using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Barber.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string catalogConnectionString)
    {
        services.AddDbContext<CatalogContext>(o => o.UseNpgsql(catalogConnectionString));
        services.AddDbContext<TenantContext>((sp, o) =>
            o.UseNpgsql(sp.GetRequiredService<ITenantProvider>().GetConnectionString()));
        return services;
    }
}

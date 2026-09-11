namespace Barber.DataAccess;

public interface ITenantProvider
{
    string GetSlug();
    string GetConnectionString();
}

namespace Barber.DataAccess;

public interface ITenantProvider
{
    string GetConnectionString();
}

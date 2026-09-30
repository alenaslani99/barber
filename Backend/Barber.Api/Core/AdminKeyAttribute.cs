using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Cryptography;
using System.Text;

namespace Barber.Api.Core;

/// <summary>
/// Guards the local-only admin API with a shared key from <c>Admin:ApiKey</c>.
/// Outside Development, or with no key configured, the endpoints answer 404 as if
/// they did not exist.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AdminKeyAttribute : Attribute, IAuthorizationFilter
{
    public const string HeaderName = "X-Admin-Key";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        IServiceProvider services = context.HttpContext.RequestServices;
        IWebHostEnvironment environment = services.GetRequiredService<IWebHostEnvironment>();
        string? expected = services.GetRequiredService<IConfiguration>()["Admin:ApiKey"];

        if (!environment.IsDevelopment() || string.IsNullOrWhiteSpace(expected))
        {
            context.Result = new NotFoundResult();
            return;
        }

        string provided = context.HttpContext.Request.Headers[HeaderName].ToString();
        bool matches = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(provided),
            Encoding.UTF8.GetBytes(expected));
        if (!matches)
            context.Result = new UnauthorizedResult();
    }
}

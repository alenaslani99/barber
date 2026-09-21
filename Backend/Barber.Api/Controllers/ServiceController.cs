using Barber.Api.DTO;
using Barber.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barber.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ServiceController(GetServicesHandler services) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<ServiceResponse>>> List(CancellationToken ct)
    {
        List<ServiceListItem> items = await services.HandleAsync(ct);
        List<ServiceResponse> response = items
            .Select(i => new ServiceResponse(i.Id, i.Name, i.DurationMinutes, i.Price))
            .ToList();
        return Ok(response);
    }
}

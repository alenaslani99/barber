using Barber.Api.DTO;
using Barber.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barber.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BarbershopController(GetShopHandler shop) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ShopResponse>> Get(CancellationToken ct)
    {
        ShopResult? result = await shop.HandleAsync(ct);
        if (result is null)
            return NotFound();
        return Ok(new ShopResponse(
            result.Id,
            result.Name,
            result.Tagline,
            result.Description,
            result.Address,
            result.Phone,
            result.Hours.Select(h => new ShopHourResponse(h.Day, h.Open, h.Close, h.Closed)).ToList(),
            result.Staff.Select(s => new BarberResponse(s.Id, s.FirstName, s.LastName, s.Seniority)).ToList(),
            result.Services.Select(s => new ServiceResponse(s.Id, s.Name, s.DurationMinutes, s.Price)).ToList()));
    }
}

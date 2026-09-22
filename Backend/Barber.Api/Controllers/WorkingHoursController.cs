using Barber.Api.DTO;
using Barber.Application.UseCases;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Barber.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Owner")]
public sealed class WorkingHoursController(
    GetWorkingHoursHandler list,
    UpdateWorkingHoursHandler update,
    IValidator<UpdateWorkingHoursCommand> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<WorkingHoursResponse>>> List(
        [FromQuery] Guid? barbershopId, CancellationToken ct)
    {
        if (!barbershopId.HasValue)
            return BadRequest();
        List<ShopHoursRow>? rows = await list.HandleAsync(barbershopId.Value, ct);
        if (rows is null)
            return NotFound();
        return Ok(rows
            .Select(r => new WorkingHoursResponse(r.Day, r.Open.ToString("HH:mm"), r.Close.ToString("HH:mm"), r.IsClosed))
            .ToList());
    }

    [HttpPut]
    public async Task<IActionResult> Replace(UpdateWorkingHoursRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
            return Unauthorized();
        string email = User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;

        List<WorkingHoursInput> hours = [];
        foreach (WorkingHoursInputRequest input in request.Hours)
        {
            if (!TimeOnly.TryParse(input.Open, out TimeOnly open) ||
                !TimeOnly.TryParse(input.Close, out TimeOnly close))
                return BadRequest();
            hours.Add(new WorkingHoursInput(input.Day, open, close, input.IsClosed));
        }

        UpdateWorkingHoursCommand command = new(request.BarbershopId, hours, userId, email);
        await validator.ValidateAndThrowAsync(command, ct);
        bool updated = await update.HandleAsync(command, ct);
        if (!updated)
            return NotFound();
        return NoContent();
    }
}

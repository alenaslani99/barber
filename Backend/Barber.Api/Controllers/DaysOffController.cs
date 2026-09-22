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
public sealed class DaysOffController(
    GetDaysOffHandler list,
    CreateDayOffHandler create,
    DeleteDayOffHandler remove,
    IValidator<CreateDayOffCommand> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DayOffResponse>>> List(
        [FromQuery] Guid? staffId, [FromQuery] DateOnly? from, CancellationToken ct)
    {
        DateOnly fromValue = from ?? DateOnly.FromDateTime(DateTime.UtcNow);
        List<DayOffItem> items = await list.HandleAsync(new GetDaysOffQuery(staffId, fromValue), ct);
        List<DayOffResponse> response = items
            .Select(i => new DayOffResponse(i.Id, i.StaffId, i.StaffName, i.Date, i.Reason))
            .ToList();
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<DayOffCreatedResponse>> Create(CreateDayOffRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
            return Unauthorized();
        string email = User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;
        CreateDayOffCommand command = new(request.StaffId, request.Date, request.Reason, userId, email);
        await validator.ValidateAndThrowAsync(command, ct);
        Guid id = await create.HandleAsync(command, ct);
        return Ok(new DayOffCreatedResponse(id));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
            return Unauthorized();
        string email = User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;
        bool removed = await remove.HandleAsync(new DeleteDayOffCommand(id, userId, email), ct);
        if (!removed)
            return NotFound();
        return NoContent();
    }
}

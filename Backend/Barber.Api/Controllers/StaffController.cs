using Barber.Api.DTO;
using Barber.Application.UseCases;
using Barber.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Barber.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Owner")]
public sealed class StaffController(
    CreateStaffHandler create,
    GetBarbersHandler barbers,
    UpdateStaffHandler update,
    IValidator<CreateStaffCommand> validator) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<BarberResponse>>> List([FromQuery] bool? includeInactive, CancellationToken ct)
    {
        bool all = includeInactive == true && User.IsInRole(nameof(UserRole.Owner));
        List<BarberListItem> items = await barbers.HandleAsync(all, ct);
        List<BarberResponse> response = items
            .Select(i => new BarberResponse(i.Id, i.FirstName, i.LastName, i.Seniority, i.IsActive))
            .ToList();
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<StaffResponse>> Create(CreateStaffRequest request, CancellationToken ct)
    {
        CreateStaffCommand command = new(request.Email, request.BarbershopId, request.SeniorityId);
        await validator.ValidateAndThrowAsync(command, ct);
        Staff staff = await create.HandleAsync(command, ct);
        return Ok(new StaffResponse(staff.Id, staff.UserId, staff.BarbershopId, staff.SeniorityId, staff.IsActive));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<StaffResponse>> Update(Guid id, UpdateStaffRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
            return Unauthorized();
        string email = User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;
        Staff? staff = await update.HandleAsync(new UpdateStaffCommand(id, request.IsActive, userId, email), ct);
        if (staff is null)
            return NotFound();
        return Ok(new StaffResponse(staff.Id, staff.UserId, staff.BarbershopId, staff.SeniorityId, staff.IsActive));
    }
}

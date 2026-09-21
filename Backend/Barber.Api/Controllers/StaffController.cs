using Barber.Api.DTO;
using Barber.Application.UseCases;
using Barber.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Barber.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Owner")]
public sealed class StaffController(
    CreateStaffHandler create,
    GetBarbersHandler barbers,
    IValidator<CreateStaffCommand> validator) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<BarberResponse>>> List(CancellationToken ct)
    {
        List<BarberListItem> items = await barbers.HandleAsync(ct);
        List<BarberResponse> response = items
            .Select(i => new BarberResponse(i.Id, i.FirstName, i.LastName, i.Seniority))
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
}

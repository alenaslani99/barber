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
    IValidator<CreateStaffCommand> validator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<StaffResponse>> Create(CreateStaffRequest request, CancellationToken ct)
    {
        CreateStaffCommand command = new(request.Email, request.BarbershopId, request.SeniorityId);
        await validator.ValidateAndThrowAsync(command, ct);
        Staff staff = await create.HandleAsync(command, ct);
        return Ok(new StaffResponse(staff.Id, staff.UserId, staff.BarbershopId, staff.SeniorityId, staff.IsActive));
    }
}

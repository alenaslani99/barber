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
public sealed class ServiceController(
    GetServicesHandler services,
    CreateServiceHandler create,
    UpdateServiceHandler update,
    IValidator<CreateServiceCommand> createValidator,
    IValidator<UpdateServiceCommand> updateValidator) : ControllerBase
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

    [Authorize(Roles = "Owner")]
    [HttpPost]
    public async Task<ActionResult<ServiceDetailResponse>> Create(CreateServiceRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
            return Unauthorized();
        string email = User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;
        CreateServiceCommand command = new(
            request.BarbershopId, request.Name, request.Price,
            request.DurationMinutes, request.SlotMinutes, userId, email);
        await createValidator.ValidateAndThrowAsync(command, ct);
        ServiceDetailResult result = await create.HandleAsync(command, ct);
        return Ok(new ServiceDetailResponse(
            result.Id, result.Name, result.Price, result.DurationMinutes, result.SlotMinutes, result.IsActive));
    }

    [Authorize(Roles = "Owner")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ServiceDetailResponse>> Update(
        Guid id, UpdateServiceRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
            return Unauthorized();
        string email = User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;
        UpdateServiceCommand command = new(
            id, request.Name, request.Price, request.DurationMinutes,
            request.SlotMinutes, request.IsActive, userId, email);
        await updateValidator.ValidateAndThrowAsync(command, ct);
        ServiceDetailResult? result = await update.HandleAsync(command, ct);
        if (result is null)
            return NotFound();
        return Ok(new ServiceDetailResponse(
            result.Id, result.Name, result.Price, result.DurationMinutes, result.SlotMinutes, result.IsActive));
    }
}

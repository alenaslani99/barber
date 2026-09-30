using Barber.Api.Core;
using Barber.Api.DTO;
using Barber.Application.UseCases;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Barber.Api.Controllers;

[ApiController]
[Route("api/admin/tenants")]
[AdminKey]
[Tags("Admin")]
public sealed class AdminTenantsController(
    GetTenantsHandler tenants,
    GetTenantSetupHandler setup,
    ProvisionTenantHandler provision,
    IValidator<ProvisionTenantCommand> provisionValidator) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List every tenant in the catalog, newest first.")]
    [ProducesResponseType<List<TenantResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<TenantResponse>>> List(CancellationToken ct)
    {
        List<TenantResult> items = await tenants.HandleAsync(ct);
        return Ok(items.Select(ToResponse).ToList());
    }

    [HttpGet("{slug}")]
    [EndpointSummary("Get one tenant with its onboarding progress.")]
    [ProducesResponseType<TenantSetupResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantSetupResponse>> Get(string slug, CancellationToken ct)
    {
        TenantSetupResult? result = await setup.HandleAsync(slug, ct);
        if (result is null)
            return NotFound();
        return Ok(new TenantSetupResponse(
            ToResponse(result.Tenant), result.HasShop, result.HasOwner, result.StaffCount, result.ServiceCount));
    }

    [HttpPost]
    [EndpointSummary("Create the tenant database, run migrations and register it in the catalog.")]
    [ProducesResponseType<TenantResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TenantResponse>> Create(CreateTenantRequest request, CancellationToken ct)
    {
        ProvisionTenantCommand command = new(request.Slug, request.DatabaseName);
        await provisionValidator.ValidateAndThrowAsync(command, ct);
        TenantResult result = await provision.HandleAsync(command, ct);
        return CreatedAtAction(nameof(Get), new { slug = result.Slug }, ToResponse(result));
    }

    private static TenantResponse ToResponse(TenantResult t) =>
        new(t.Id, t.Slug, t.DatabaseName, t.IsActive, t.CreatedAt);
}

using Barber.Api.Core;
using Barber.Api.DTO;
using Barber.Application.UseCases;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Barber.Api.Controllers;

/// <summary>
/// Onboarding steps for one tenant. The <c>{tenantSlug}</c> route value is what
/// <see cref="TenantProvider"/> resolves the tenant database from.
/// </summary>
[ApiController]
[Route("api/admin/tenants/{" + TenantProvider.RouteKey + "}")]
[AdminKey]
[Tags("Admin")]
public sealed class AdminTenantSetupController(
    GetShopDetailsHandler getShop,
    UpsertShopHandler upsertShop,
    GetOwnerHandler getOwner,
    CreateOwnerHandler createOwner,
    IValidator<UpsertShopCommand> shopValidator,
    IValidator<CreateOwnerCommand> ownerValidator) : ControllerBase
{
    private const string DefaultTimeZone = "Europe/Berlin";

    [HttpGet("shop")]
    [EndpointSummary("Get the tenant's barbershop details.")]
    [ProducesResponseType<ShopDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShopDetailsResponse>> GetShop(CancellationToken ct)
    {
        ShopDetailsResult? shop = await getShop.HandleAsync(ct);
        if (shop is null)
            return NotFound();
        return Ok(ToResponse(shop));
    }

    [HttpPut("shop")]
    [EndpointSummary("Create or update the barbershop. First call also sets Mon-Sat 09:00-19:00 hours.")]
    [ProducesResponseType<ShopDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShopDetailsResponse>> UpsertShop(UpsertShopRequest request, CancellationToken ct)
    {
        UpsertShopCommand command = new(
            request.Name.Trim(),
            request.Address,
            request.Phone,
            request.Tagline?.Trim() ?? string.Empty,
            request.Description?.Trim() ?? string.Empty,
            string.IsNullOrWhiteSpace(request.TimeZone) ? DefaultTimeZone : request.TimeZone.Trim());
        await shopValidator.ValidateAndThrowAsync(command, ct);
        ShopDetailsResult shop = await upsertShop.HandleAsync(command, ct);
        return Ok(ToResponse(shop));
    }

    [HttpGet("owner")]
    [EndpointSummary("Get the shop owner account (never includes a password).")]
    [ProducesResponseType<OwnerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OwnerResponse>> GetOwner(CancellationToken ct)
    {
        OwnerResult? owner = await getOwner.HandleAsync(ct);
        if (owner is null)
            return NotFound();
        return Ok(new OwnerResponse(owner.Id, owner.FirstName, owner.LastName, owner.Email, owner.Phone));
    }

    [HttpPost("owner")]
    [EndpointSummary("Create the owner account with a generated password, returned only in this response.")]
    [ProducesResponseType<CredentialsResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CredentialsResponse>> CreateOwner(CreateOwnerRequest request, CancellationToken ct)
    {
        CreateOwnerCommand command = new(
            request.FirstName.Trim(), request.LastName.Trim(), request.Email.Trim(), request.Phone.Trim());
        await ownerValidator.ValidateAndThrowAsync(command, ct);
        CredentialsResult result = await createOwner.HandleAsync(command, ct);
        Response.Headers.CacheControl = "no-store";
        return CreatedAtAction(
            nameof(GetOwner),
            new { tenantSlug = RouteData.Values[TenantProvider.RouteKey] },
            new CredentialsResponse(result.UserId, result.FirstName, result.LastName, result.Email, result.Password));
    }

    private static ShopDetailsResponse ToResponse(ShopDetailsResult s) =>
        new(s.Id, s.Name, s.Address, s.Phone, s.Tagline, s.Description, s.TimeZone);
}

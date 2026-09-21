using Barber.Api.DTO;
using Barber.Application.UseCases;
using Barber.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Barber.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class BookingController(
    GetMyBookingsHandler mine,
    GetBookingsHandler all,
    CreateBookingHandler create,
    IValidator<CreateBookingCommand> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AdminBookingsResponse>> List(
        [FromQuery] Guid[] staffId,
        [FromQuery] string[] status,
        [FromQuery] bool? upcoming,
        [FromQuery] int? skip,
        [FromQuery] int? take,
        CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
            return Unauthorized();
        bool ownerView = User.IsInRole(nameof(UserRole.Owner));
        bool barberView = User.IsInRole(nameof(UserRole.Barber));
        if (!ownerView && !barberView)
            return NotFound();

        List<BookingStatus> statuses = [];
        foreach (string name in status)
        {
            if (!Enum.TryParse<BookingStatus>(name, true, out BookingStatus parsed))
                return BadRequest();
            statuses.Add(parsed);
        }

        int skipValue = skip ?? 0;
        int takeValue = take ?? 10;
        if (skipValue < 0 || takeValue < 1 || takeValue > 50)
            return BadRequest();

        GetBookingsQuery query = new(
            userId,
            ownerView,
            ownerView ? staffId.ToList() : [],
            statuses,
            upcoming ?? true,
            skipValue,
            takeValue);
        BookingsResult result = await all.HandleAsync(query, ct);
        List<AdminBookingResponse> items = result.Items
            .Select(i => new AdminBookingResponse(i.Id, i.ServiceName, i.BarberName, i.ClientName, i.StartsAt, i.EndsAt, i.Status))
            .ToList();
        return Ok(new AdminBookingsResponse(items, result.Total));
    }

    [HttpGet("mine")]
    public async Task<ActionResult<MyBookingsResponse>> Mine([FromQuery] int? skip, [FromQuery] int? take, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
            return Unauthorized();
        int skipValue = skip ?? 0;
        int takeValue = take ?? 5;
        if (skipValue < 0 || takeValue < 1 || takeValue > 50)
            return BadRequest();
        MyBookingsResult result = await mine.HandleAsync(new GetMyBookingsQuery(userId, skipValue, takeValue), ct);
        List<MyBookingResponse> items = result.Items
            .Select(i => new MyBookingResponse(i.Id, i.ServiceName, i.BarberName, i.StartsAt, i.EndsAt, i.Status))
            .ToList();
        return Ok(new MyBookingsResponse(items, result.Total));
    }

    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create(CreateBookingRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
            return Unauthorized();
        CreateBookingCommand command = new(userId, request.ServiceId, request.StaffId, request.StartsAt, request.Notes);
        await validator.ValidateAndThrowAsync(command, ct);
        BookingResult result = await create.HandleAsync(command, ct);
        return Ok(new BookingResponse(result.Id, result.Status, result.StartsAt, result.EndsAt));
    }
}

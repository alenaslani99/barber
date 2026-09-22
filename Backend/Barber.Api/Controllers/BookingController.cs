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
    GetAvailabilityHandler availability,
    CreateBookingHandler create,
    UpdateBookingStatusHandler changeStatus,
    IValidator<CreateBookingCommand> createValidator,
    IValidator<UpdateBookingStatusCommand> statusValidator) : ControllerBase
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
        CreateBookingCommand createCommand = new(userId, request.ServiceId, request.StaffId, request.StartsAt, request.Notes);
        await createValidator.ValidateAndThrowAsync(createCommand, ct);
        BookingResult result = await create.HandleAsync(createCommand, ct);
        return Ok(new BookingResponse(result.Id, result.Status, result.StartsAt, result.EndsAt));
    }

    [Authorize]
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<AdminBookingResponse>> ChangeStatus(
        Guid id, ChangeBookingStatusRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
            return Unauthorized();
        bool ownerView = User.IsInRole(nameof(UserRole.Owner));
        if (!ownerView && !User.IsInRole(nameof(UserRole.Barber)))
            return NotFound();
        UpdateBookingStatusCommand command = new(id, userId, ownerView, request.Status);
        await statusValidator.ValidateAndThrowAsync(command, ct);
        BookingListItem? item = await changeStatus.HandleAsync(command, ct);
        if (item is null)
            return NotFound();
        return Ok(new AdminBookingResponse(
            item.Id, item.ServiceName, item.BarberName, item.ClientName,
            item.StartsAt, item.EndsAt, item.Status));
    }

    [AllowAnonymous]
    [HttpGet("availability")]
    public async Task<ActionResult<AvailabilityResponse>> Availability(
        [FromQuery] Guid? staffId, [FromQuery] DateOnly? date, [FromQuery] Guid? serviceId, CancellationToken ct)
    {
        if (!staffId.HasValue || !date.HasValue)
            return BadRequest();
        AvailabilityResult? result = await availability.HandleAsync(
            new GetAvailabilityQuery(staffId.Value, date.Value, serviceId), ct);
        if (result is null)
            return NotFound();
        return Ok(new AvailabilityResponse(
            result.Date, result.Closed, result.Open, result.Close, result.SlotMinutes, result.Taken));
    }
}

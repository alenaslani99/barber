using Barber.Application.UseCases;
using Barber.Domain;
using FluentValidation;

namespace Barber.Application.Validators;

public sealed class UpdateBookingStatusValidator : AbstractValidator<UpdateBookingStatusCommand>
{
    public UpdateBookingStatusValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<BookingStatus>(s, true, out _))
            .WithMessage("Unknown status.");
    }
}

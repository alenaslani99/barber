using Barber.Application.UseCases;
using FluentValidation;

namespace Barber.Application.Validators;

public sealed class UpdateServiceValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).When(x => x.Name is not null);
        RuleFor(x => x.Price).GreaterThan(0).When(x => x.Price.HasValue);
        RuleFor(x => x.DurationMinutes).InclusiveBetween(5, 480).When(x => x.DurationMinutes.HasValue);
        RuleFor(x => x.SlotMinutes).InclusiveBetween(5, 120).When(x => x.SlotMinutes.HasValue);
    }
}

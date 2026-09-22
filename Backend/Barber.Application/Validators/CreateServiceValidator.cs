using Barber.Application.UseCases;
using FluentValidation;

namespace Barber.Application.Validators;

public sealed class CreateServiceValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceValidator()
    {
        RuleFor(x => x.BarbershopId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.DurationMinutes).InclusiveBetween(5, 480);
        RuleFor(x => x.SlotMinutes).InclusiveBetween(5, 120);
    }
}

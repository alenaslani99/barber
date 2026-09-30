using Barber.Application.UseCases;
using FluentValidation;

namespace Barber.Application.Validators;

public sealed class CreateShopServiceValidator : AbstractValidator<CreateShopServiceCommand>
{
    public CreateShopServiceValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0).PrecisionScale(10, 2, ignoreTrailingZeros: true);
        RuleFor(x => x.DurationMinutes).InclusiveBetween(5, 480);
        RuleFor(x => x.SlotMinutes).InclusiveBetween(5, 120);
    }
}

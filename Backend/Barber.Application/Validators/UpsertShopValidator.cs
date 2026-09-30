using Barber.Application.UseCases;
using FluentValidation;

namespace Barber.Application.Validators;

public sealed class UpsertShopValidator : AbstractValidator<UpsertShopCommand>
{
    public UpsertShopValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Phone).MaximumLength(32);
        RuleFor(x => x.Tagline).NotNull().MaximumLength(100);
        RuleFor(x => x.Description).NotNull().MaximumLength(500);
        RuleFor(x => x.TimeZone)
            .NotEmpty()
            .MaximumLength(64)
            .Must(tz => TimeZoneInfo.TryFindSystemTimeZoneById(tz, out _))
            .WithMessage("Unknown time zone. Use an IANA id such as Europe/Berlin.");
    }
}

using Barber.Application.UseCases;
using FluentValidation;

namespace Barber.Application.Validators;

public sealed class CreateDayOffValidator : AbstractValidator<CreateDayOffCommand>
{
    public CreateDayOffValidator()
    {
        RuleFor(x => x.StaffId).NotEmpty();
        RuleFor(x => x.Date).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
        RuleFor(x => x.Reason).MaximumLength(200);
    }
}

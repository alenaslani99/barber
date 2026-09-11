using Barber.Application.UseCases;
using FluentValidation;

namespace Barber.Application.Validators;

public sealed class CreateStaffValidator : AbstractValidator<CreateStaffCommand>
{
    public CreateStaffValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.BarbershopId).NotEmpty();
    }
}

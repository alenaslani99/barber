using Barber.Application.UseCases;
using FluentValidation;

namespace Barber.Application.Validators;

public sealed class UpdateWorkingHoursValidator : AbstractValidator<UpdateWorkingHoursCommand>
{
    public UpdateWorkingHoursValidator()
    {
        RuleFor(x => x.BarbershopId).NotEmpty();
        RuleFor(x => x.Hours).NotEmpty().Must(h => h.Count == 7).WithMessage("Must contain all 7 days.");
        RuleForEach(x => x.Hours).ChildRules(day =>
        {
            day.RuleFor(h => h.Day).IsInEnum();
            day.RuleFor(h => h)
                .Must(h => h.IsClosed || h.Open < h.Close)
                .WithMessage("Open must be before close unless closed.");
        });
        RuleFor(x => x.Hours)
            .Must(h => h.Select(d => d.Day).Distinct().Count() == h.Count)
            .WithMessage("Days must be unique.")
            .When(x => x.Hours.Count == 7);
    }
}

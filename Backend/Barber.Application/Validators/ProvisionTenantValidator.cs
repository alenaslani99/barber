using Barber.Application.UseCases;
using FluentValidation;

namespace Barber.Application.Validators;

public sealed class ProvisionTenantValidator : AbstractValidator<ProvisionTenantCommand>
{
    public ProvisionTenantValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .Matches("^[a-z0-9][a-z0-9-]{1,62}$")
            .WithMessage("Lowercase letters, numbers and dashes only (2-63 chars).");

        // The database name ends up in DDL, which cannot be parameterized. This whitelist
        // is what keeps it safe, so do not loosen it.
        RuleFor(x => x.DatabaseName)
            .NotEmpty()
            .Matches("^[a-z][a-z0-9_]{2,62}$")
            .WithMessage("Start with a letter; lowercase letters, numbers and underscores only (3-63 chars).");
    }
}

using FluentValidation;
using MeAjudaAi.Modules.Providers.Application.Commands;

namespace MeAjudaAi.Modules.Providers.Application.Validators;

public class RegisterProviderCommandValidator : AbstractValidator<RegisterProviderCommand>
{
    public RegisterProviderCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DocumentNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Type)
            .IsInEnum();
    }
}

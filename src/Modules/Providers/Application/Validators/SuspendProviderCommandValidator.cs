using FluentValidation;

namespace MeAjudaAi.Modules.Providers.Application.Validators;

/// <summary>
/// Validador para o comando de suspensão de prestador de serviços.
/// </summary>
public class SuspendProviderCommandValidator : AbstractValidator<Commands.SuspendProviderCommand>
{
    public SuspendProviderCommandValidator()
    {
        RuleFor(x => x.ProviderId)
            .NotEmpty()
            .WithMessage("Provider ID is required");

        RuleFor(x => x.SuspendedBy)
            .NotEmpty()
            .WithMessage("SuspendedBy is required for audit purposes")
            .MaximumLength(255)
            .WithMessage("SuspendedBy cannot exceed 255 characters");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required for audit purposes")
            .MinimumLength(10)
            .WithMessage("Reason must be at least 10 characters")
            .MaximumLength(1000)
            .WithMessage("Reason cannot exceed 1000 characters");
    }
}

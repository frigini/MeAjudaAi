using FluentValidation;

namespace MeAjudaAi.Modules.Providers.Application.Validators;

/// <summary>
/// Validador para o comando de rejeição de prestador de serviços.
/// </summary>
public class RejectProviderCommandValidator : AbstractValidator<Commands.RejectProviderCommand>
{
    public RejectProviderCommandValidator()
    {
        RuleFor(x => x.ProviderId)
            .NotEmpty()
            .WithMessage("Provider ID is required");

        RuleFor(x => x.RejectedBy)
            .NotEmpty()
            .WithMessage("RejectedBy is required for audit purposes")
            .MaximumLength(255)
            .WithMessage("RejectedBy cannot exceed 255 characters");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required for audit purposes")
            .MinimumLength(10)
            .WithMessage("Reason must be at least 10 characters")
            .MaximumLength(1000)
            .WithMessage("Reason cannot exceed 1000 characters");
    }
}

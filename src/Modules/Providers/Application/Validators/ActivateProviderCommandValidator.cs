using FluentValidation;
using MeAjudaAi.Modules.Providers.Application.Commands;

namespace MeAjudaAi.Modules.Providers.Application.Validators;

/// <summary>
/// Validador para o comando de ativação de prestador de serviços.
/// </summary>
public class ActivateProviderCommandValidator : AbstractValidator<ActivateProviderCommand>
{
    public ActivateProviderCommandValidator()
    {
        RuleFor(x => x.ProviderId)
            .NotEmpty()
            .WithMessage("O ID do prestador é obrigatório.");
    }
}

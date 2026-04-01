using FluentValidation;
using MeAjudaAi.Modules.Providers.Application.Commands;

namespace MeAjudaAi.Modules.Providers.Application.Validators;

/// <summary>
/// Validador para o comando de registro inicial de prestador de serviços.
/// </summary>
public class RegisterProviderCommandValidator : AbstractValidator<RegisterProviderCommand>
{
    public RegisterProviderCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório")
            .MaximumLength(100).WithMessage("O nome não pode exceder 100 caracteres");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("O número do documento é obrigatório")
            .MaximumLength(20).WithMessage("O número do documento não pode exceder 20 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório")
            .EmailAddress().WithMessage("E-mail inválido");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("O número de telefone é obrigatório")
            .MaximumLength(20).WithMessage("O número de telefone não pode exceder 20 caracteres")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Número de telefone inválido");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de prestador inválido");
    }
}

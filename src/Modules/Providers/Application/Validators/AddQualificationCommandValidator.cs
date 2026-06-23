using FluentValidation;
using MeAjudaAi.Modules.Providers.Application.Commands;

namespace MeAjudaAi.Modules.Providers.Application.Validators;

/// <summary>
/// Validador para o comando de adição de qualificação a prestador de serviços.
/// </summary>
public class AddQualificationCommandValidator : AbstractValidator<AddQualificationCommand>
{
    public AddQualificationCommandValidator()
    {
        RuleFor(x => x.ProviderId)
            .NotEmpty()
            .WithMessage("O ID do prestador é obrigatório.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("O nome da qualificação é obrigatório.")
            .MinimumLength(2)
            .WithMessage("O nome da qualificação deve ter pelo menos 2 caracteres.")
            .MaximumLength(200)
            .WithMessage("O nome da qualificação não pode exceder 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("A descrição não pode exceder 1000 caracteres.")
            .When(x => x.Description != null);

        RuleFor(x => x.IssuingOrganization)
            .MaximumLength(200)
            .WithMessage("A organização emissora não pode exceder 200 caracteres.")
            .When(x => x.IssuingOrganization != null);

        RuleFor(x => x.IssueDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("A data de emissão não pode ser no futuro.")
            .When(x => x.IssueDate.HasValue);

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(x => x.IssueDate)
            .WithMessage("A data de expiração deve ser posterior à data de emissão.")
            .When(x => x.IssueDate.HasValue && x.ExpirationDate.HasValue);

        RuleFor(x => x.DocumentNumber)
            .MaximumLength(100)
            .WithMessage("O número do documento não pode exceder 100 caracteres.")
            .When(x => x.DocumentNumber != null);
    }
}

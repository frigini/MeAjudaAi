using FluentValidation;
using MeAjudaAi.Modules.Communications.Application.Commands;

namespace MeAjudaAi.Modules.Communications.Application.Validators;

public class SetEmailTemplateStatusCommandValidator : AbstractValidator<SetEmailTemplateStatusCommand>
{
    public SetEmailTemplateStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O identificador do template é obrigatório.");
    }
}

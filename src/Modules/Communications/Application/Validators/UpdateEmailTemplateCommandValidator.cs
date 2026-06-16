using FluentValidation;
using MeAjudaAi.Modules.Communications.Application.Commands;

namespace MeAjudaAi.Modules.Communications.Application.Validators;

public class UpdateEmailTemplateCommandValidator : AbstractValidator<UpdateEmailTemplateCommand>
{
    public UpdateEmailTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O identificador do template é obrigatório.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("O assunto do template é obrigatório.")
            .MaximumLength(200).WithMessage("O assunto do template não pode exceder 200 caracteres.");

        RuleFor(x => x.HtmlBody)
            .NotEmpty().WithMessage("O corpo HTML do template é obrigatório.");

        RuleFor(x => x.TextBody)
            .NotEmpty().WithMessage("O corpo de texto do template é obrigatório.");
    }
}

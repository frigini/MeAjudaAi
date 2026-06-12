using FluentValidation;
using MeAjudaAi.Modules.Communications.Application.Commands;

namespace MeAjudaAi.Modules.Communications.Application.Validators;

public class CreateEmailTemplateCommandValidator : AbstractValidator<CreateEmailTemplateCommand>
{
    public CreateEmailTemplateCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("A chave do template é obrigatória.")
            .MaximumLength(100).WithMessage("A chave do template não pode exceder 100 caracteres.")
            .Matches("^[a-z0-9_]+$").WithMessage("A chave do template deve conter apenas letras minúsculas, números e underscores.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("O assunto do template é obrigatório.")
            .MaximumLength(200).WithMessage("O assunto do template não pode exceder 200 caracteres.");

        RuleFor(x => x.HtmlBody)
            .NotEmpty().WithMessage("O corpo HTML do template é obrigatório.");

        RuleFor(x => x.TextBody)
            .NotEmpty().WithMessage("O corpo de texto do template é obrigatório.");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("O idioma do template é obrigatório.")
            .Matches(new System.Text.RegularExpressions.Regex("^[a-z]{2}(-[a-z]{2})?$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            .WithMessage("O idioma deve estar no formato 'pt', 'pt-BR', 'en', etc.");
    }
}

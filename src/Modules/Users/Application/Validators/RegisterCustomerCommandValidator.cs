using FluentValidation;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Users.Application.Validators;

public class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .Must(name => !string.IsNullOrWhiteSpace(name) && System.Text.RegularExpressions.Regex.IsMatch(name, @"\p{L}"))
            .WithMessage("Nome deve conter pelo menos uma letra e não pode ser apenas espaços")
            .MinimumLength(ValidationConstants.UserLimits.FirstNameMinLength + ValidationConstants.UserLimits.LastNameMinLength).WithMessage($"Nome deve ter pelo menos {ValidationConstants.UserLimits.FirstNameMinLength + ValidationConstants.UserLimits.LastNameMinLength} caracteres")
            .MaximumLength(ValidationConstants.UserLimits.FirstNameMaxLength + ValidationConstants.UserLimits.LastNameMaxLength).WithMessage($"Nome deve ter no máximo {ValidationConstants.UserLimits.FirstNameMaxLength + ValidationConstants.UserLimits.LastNameMaxLength} caracteres")
            .Matches(ValidationConstants.Patterns.Name).WithMessage("Nome deve conter apenas letras e espaços");
            
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(ValidationConstants.UserLimits.EmailMaxLength).WithMessage($"Email deve ter no máximo {ValidationConstants.UserLimits.EmailMaxLength} caracteres");
            
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(ValidationConstants.UserLimits.PhoneNumberMaxLength).WithMessage($"Telefone deve ter no máximo {ValidationConstants.UserLimits.PhoneNumberMaxLength} caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
            
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória");

        RuleFor(x => x.Password)
            .Matches(ValidationConstants.Patterns.Password).WithMessage("Senha deve ter pelo menos 8 caracteres, uma letra maiúscula, uma minúscula e um número")
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.TermsAccepted)
            .Equal(true).WithMessage("Você deve aceitar os termos de uso");

        RuleFor(x => x.AcceptedPrivacyPolicy)
            .Equal(true).WithMessage("Você deve aceitar a política de privacidade");
    }
}

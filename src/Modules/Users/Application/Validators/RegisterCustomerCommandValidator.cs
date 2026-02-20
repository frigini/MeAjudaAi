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
            .MaximumLength(ValidationConstants.UserLimits.FirstNameMaxLength + ValidationConstants.UserLimits.LastNameMaxLength).WithMessage($"Nome deve ter no máximo {ValidationConstants.UserLimits.FirstNameMaxLength + ValidationConstants.UserLimits.LastNameMaxLength} caracteres");
            
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(ValidationConstants.UserLimits.EmailMaxLength).WithMessage($"Email deve ter no máximo {ValidationConstants.UserLimits.EmailMaxLength} caracteres");
            
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(ValidationConstants.UserLimits.PhoneNumberMaxLength).WithMessage($"Telefone deve ter no máximo {ValidationConstants.UserLimits.PhoneNumberMaxLength} caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
            
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(ValidationConstants.PasswordLimits.MinLength).WithMessage($"Senha deve ter pelo menos {ValidationConstants.PasswordLimits.MinLength} caracteres")
            .Matches(@"[a-zA-Z]").WithMessage("A senha deve conter pelo menos uma letra")
            .Matches(@"\d").WithMessage("A senha deve conter pelo menos um número")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("A senha deve conter pelo menos um caractere especial");
    }
}

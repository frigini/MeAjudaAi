using FluentValidation;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Modules.Users.Application.Validators;

/// <summary>
/// Validator for CreateUserCommand
/// </summary>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required.Username);

        RuleFor(x => x.Username)
            .MinimumLength(ValidationConstants.UserLimits.UsernameMinLength)
            .WithMessage(ValidationMessages.Length.UsernameTooShort)
            .When(x => !string.IsNullOrWhiteSpace(x.Username));

        RuleFor(x => x.Username)
            .MaximumLength(ValidationConstants.UserLimits.UsernameMaxLength)
            .WithMessage(ValidationMessages.Length.UsernameTooLong)
            .When(x => !string.IsNullOrWhiteSpace(x.Username));

        RuleFor(x => x.Username)
            .Matches(ValidationConstants.Patterns.Username)
            .WithMessage(ValidationMessages.InvalidFormat.Username)
            .When(x => !string.IsNullOrWhiteSpace(x.Username));

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required.Email)
            .EmailAddress()
            .WithMessage(ValidationMessages.InvalidFormat.Email)
            .MaximumLength(ValidationConstants.UserLimits.EmailMaxLength)
            .WithMessage(ValidationMessages.Length.EmailTooLong);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required.FirstName);

        RuleFor(x => x.FirstName)
            .MinimumLength(ValidationConstants.UserLimits.FirstNameMinLength)
            .WithMessage(ValidationMessages.Length.FirstNameTooShort)
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.FirstName)
            .MaximumLength(ValidationConstants.UserLimits.FirstNameMaxLength)
            .WithMessage(ValidationMessages.Length.FirstNameTooLong)
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.FirstName)
            .Matches(ValidationConstants.Patterns.Name)
            .WithMessage(ValidationMessages.InvalidFormat.FirstName)
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required.LastName);

        RuleFor(x => x.LastName)
            .MinimumLength(ValidationConstants.UserLimits.LastNameMinLength)
            .WithMessage(ValidationMessages.Length.LastNameTooShort)
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.LastName)
            .MaximumLength(ValidationConstants.UserLimits.LastNameMaxLength)
            .WithMessage(ValidationMessages.Length.LastNameTooLong)
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.LastName)
            .Matches(ValidationConstants.Patterns.Name)
            .WithMessage(ValidationMessages.InvalidFormat.LastName)
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("A senha é obrigatória")
            .MinimumLength(8)
            .WithMessage("A senha deve ter pelo menos 8 caracteres")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
            .WithMessage("A senha deve conter pelo menos uma letra minúscula, uma letra maiúscula e um número");

        When(x => x.Roles != null, () =>
        {
            RuleForEach(x => x.Roles)
                .NotEmpty()
                .WithMessage("O papel não pode estar vazio")
                .Must(role => UserRoles.IsValidRole(role))
                .WithMessage($"Papel inválido. Papéis válidos: {string.Join(", ", UserRoles.BasicRoles)}");
        });

        // PhoneNumber validation (optional field)
        RuleFor(x => x.PhoneNumber)
            .Must(phone => string.IsNullOrWhiteSpace(phone) || IsValidPhoneNumber(phone))
            .WithMessage("O número de telefone deve estar no formato internacional (ex.: +5511999999999)")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        // Basic validation for international format: +[country code][number]
        // Must start with + and contain 8-15 digits
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        if (!phoneNumber.StartsWith('+'))
            return false;

        var digitsOnly = phoneNumber[1..].Replace(" ", "").Replace("-", "");
        return digitsOnly.Length >= 8 && digitsOnly.Length <= 15 && digitsOnly.All(char.IsDigit);
    }
}

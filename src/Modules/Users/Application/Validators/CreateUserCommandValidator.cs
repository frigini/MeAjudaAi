using FluentValidation;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Modules.Users.Application.Validators;

/// <summary>
/// Validador para CreateUserCommand
/// </summary>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required.Username)
            .MinimumLength(ValidationConstants.UserLimits.UsernameMinLength)
            .WithMessage(ValidationMessages.Length.UsernameTooShort)
            .MaximumLength(ValidationConstants.UserLimits.UsernameMaxLength)
            .WithMessage(ValidationMessages.Length.UsernameTooLong)
            .Matches(ValidationConstants.Patterns.Username)
            .WithMessage(ValidationMessages.InvalidFormat.Username);

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required.Email)
            .EmailAddress()
            .WithMessage(ValidationMessages.InvalidFormat.Email)
            .MaximumLength(ValidationConstants.UserLimits.EmailMaxLength)
            .WithMessage(ValidationMessages.Length.EmailTooLong);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required.FirstName)
            .MinimumLength(ValidationConstants.UserLimits.FirstNameMinLength)
            .WithMessage(ValidationMessages.Length.FirstNameTooShort)
            .MaximumLength(ValidationConstants.UserLimits.FirstNameMaxLength)
            .WithMessage(ValidationMessages.Length.FirstNameTooLong)
            .Matches(ValidationConstants.Patterns.Name)
            .WithMessage(ValidationMessages.InvalidFormat.FirstName);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required.LastName)
            .MinimumLength(ValidationConstants.UserLimits.LastNameMinLength)
            .WithMessage(ValidationMessages.Length.LastNameTooShort)
            .MaximumLength(ValidationConstants.UserLimits.LastNameMaxLength)
            .WithMessage(ValidationMessages.Length.LastNameTooLong)
            .Matches(ValidationConstants.Patterns.Name)
            .WithMessage(ValidationMessages.InvalidFormat.LastName);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required.Password)
            .MinimumLength(ValidationConstants.PasswordLimits.MinLength)
            .WithMessage(ValidationMessages.Length.PasswordTooShort)
            .Matches(ValidationConstants.Patterns.Password)
            .WithMessage(ValidationMessages.InvalidFormat.Password);

        When(x => x.Roles != null, () =>
        {
            RuleForEach(x => x.Roles)
                .NotEmpty()
                .WithMessage(ValidationMessages.Required.Role)
                .Must(role => UserRoles.IsValidRole(role))
                .WithMessage(string.Format(ValidationMessages.InvalidFormat.Role, string.Join(", ", UserRoles.AllRoles)));
        });

        // Validação de PhoneNumber (campo opcional)
        RuleFor(x => x.PhoneNumber)
            .Must(phone => string.IsNullOrWhiteSpace(phone) || PhoneNumberValidator.IsValidInternationalFormat(phone))
            .WithMessage(ValidationMessages.InvalidFormat.PhoneNumber);
    }
}

using FluentValidation;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Modules.Users.Application.Validators;

/// <summary>
/// Validator para CreateUserRequest
/// </summary>
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
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
    }
}

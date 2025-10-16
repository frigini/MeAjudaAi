using FluentValidation;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Security;

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
            .Length(ValidationConstants.UserLimits.UsernameMinLength, ValidationConstants.UserLimits.UsernameMaxLength)
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
            .Length(ValidationConstants.UserLimits.FirstNameMinLength, ValidationConstants.UserLimits.FirstNameMaxLength)
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
            .Length(ValidationConstants.UserLimits.LastNameMinLength, ValidationConstants.UserLimits.LastNameMaxLength)
            .WithMessage(ValidationMessages.Length.LastNameTooLong)
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.LastName)
            .Matches(ValidationConstants.Patterns.Name)
            .WithMessage(ValidationMessages.InvalidFormat.LastName)
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        // Manter validação de password original (não está nas constantes)
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
            .WithMessage("Password must contain at least one lowercase letter, one uppercase letter and one number");

        When(x => x.Roles != null, () =>
        {
            RuleForEach(x => x.Roles)
                .NotEmpty()
                .WithMessage("Role cannot be empty")
                .Must(role => UserRoles.IsValidRole(role))
                .WithMessage($"Invalid role. Valid roles: {string.Join(", ", UserRoles.BasicRoles)}");
        });
    }
}
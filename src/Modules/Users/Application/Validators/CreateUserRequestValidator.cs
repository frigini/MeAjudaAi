using FluentValidation;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
<<<<<<< HEAD
=======
using MeAjudaAi.Shared.Constants;
>>>>>>> 44e76d9c34933851c9d11d302fe61ae4d8806921
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
<<<<<<< HEAD
            .WithMessage("Username is required")
            .Length(3, 50)
            .WithMessage("Username must be between 3 and 50 characters")
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("Username must contain only letters, numbers, dots, hyphens or underscores");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must have a valid format")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters");

=======
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

        // Manter validação de password original (não está nas constantes)
>>>>>>> 44e76d9c34933851c9d11d302fe61ae4d8806921
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
            .WithMessage("Password must contain at least one lowercase letter, one uppercase letter and one number");

<<<<<<< HEAD
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .Length(2, 100)
            .WithMessage("First name must be between 2 and 100 characters")
            .Matches("^[a-zA-ZÀ-ÿ\\s]+$")
            .WithMessage("First name must contain only letters and spaces");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .Length(2, 100)
            .WithMessage("Last name must be between 2 and 100 characters")
            .Matches("^[a-zA-ZÀ-ÿ\\s]+$")
            .WithMessage("Last name must contain only letters and spaces");

=======
>>>>>>> 44e76d9c34933851c9d11d302fe61ae4d8806921
        When(x => x.Roles != null, () =>
        {
            RuleForEach(x => x.Roles)
                .NotEmpty()
                .WithMessage("Role cannot be empty")
                .Must(role => UserRoles.IsValidRole(role))
                .WithMessage($"Invalid role. Valid roles: {string.Join(", ", UserRoles.BasicRoles)}");
        });
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> 44e76d9c34933851c9d11d302fe61ae4d8806921

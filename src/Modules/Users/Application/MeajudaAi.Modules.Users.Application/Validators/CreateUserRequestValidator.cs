using FluentValidation;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
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

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
            .WithMessage("Password must contain at least one lowercase letter, one uppercase letter and one number");

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

        When(x => x.Roles != null, () => {
            RuleForEach(x => x.Roles)
                .NotEmpty()
                .WithMessage("Role cannot be empty")
                .Must(role => UserRoles.IsValidRole(role))
                .WithMessage($"Invalid role. Valid roles: {string.Join(", ", UserRoles.BasicRoles)}");
        });
    }
}
using FluentValidation;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Shared.Constants;

namespace MeAjudaAi.Modules.Users.Application.Validators;

/// <summary>
/// Validator para UpdateUserProfileRequest
/// </summary>
public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {
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

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required.Email)
            .EmailAddress()
            .WithMessage(ValidationMessages.InvalidFormat.Email)
            .MaximumLength(ValidationConstants.UserLimits.EmailMaxLength)
            .WithMessage(ValidationMessages.Length.EmailTooLong);

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

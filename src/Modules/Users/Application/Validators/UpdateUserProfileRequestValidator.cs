using FluentValidation;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
<<<<<<< HEAD
=======
using MeAjudaAi.Shared.Constants;
>>>>>>> 44e76d9c34933851c9d11d302fe61ae4d8806921

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
<<<<<<< HEAD
            .WithMessage("Nome é obrigatório")
            .Length(2, 100)
            .WithMessage("Nome deve ter entre 2 e 100 caracteres")
            .Matches("^[a-zA-ZÀ-ÿ\\s]+$")
            .WithMessage("Nome deve conter apenas letras e espaços");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Sobrenome é obrigatório")
            .Length(2, 100)
            .WithMessage("Sobrenome deve ter entre 2 e 100 caracteres")
            .Matches("^[a-zA-ZÀ-ÿ\\s]+$")
            .WithMessage("Sobrenome deve conter apenas letras e espaços");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email é obrigatório")
            .EmailAddress()
            .WithMessage("Email deve ter um formato válido")
            .MaximumLength(255)
            .WithMessage("Email não pode ter mais de 255 caracteres");
    }
}
=======
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
    }
}
>>>>>>> 44e76d9c34933851c9d11d302fe61ae4d8806921

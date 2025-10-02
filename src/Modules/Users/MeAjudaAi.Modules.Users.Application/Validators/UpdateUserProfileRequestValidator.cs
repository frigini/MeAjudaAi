using FluentValidation;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;

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
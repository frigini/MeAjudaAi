using FluentValidation;
using MeAjudaAi.Modules.Locations.Application.Commands;

namespace MeAjudaAi.Modules.Locations.Application.Validators;

/// <summary>
/// Validator para UpdateAllowedCityCommand
/// </summary>
public class UpdateAllowedCityCommandValidator : AbstractValidator<UpdateAllowedCityCommand>
{
    public UpdateAllowedCityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID da cidade é obrigatório");

        RuleFor(x => x.CityName)
            .NotEmpty()
            .WithMessage("Nome da cidade é obrigatório")
            .MaximumLength(100)
            .WithMessage("Nome da cidade deve ter no máximo 100 caracteres");

        RuleFor(x => x.StateSigla)
            .NotEmpty()
            .WithMessage("Sigla do estado é obrigatória")
            .Length(2)
            .WithMessage("Sigla do estado deve ter exatamente 2 caracteres")
            .Matches("^[A-Z]{2}$")
            .WithMessage("Sigla do estado deve conter apenas letras maiúsculas");

        RuleFor(x => x.IbgeCode)
            .GreaterThan(0)
            .WithMessage("Código IBGE deve ser maior que zero")
            .When(x => x.IbgeCode.HasValue);
    }
}

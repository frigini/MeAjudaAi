using FluentValidation;
using MeAjudaAi.Modules.Locations.Application.Commands;

namespace MeAjudaAi.Modules.Locations.Application.Validators;

/// <summary>
/// Validator para DeleteAllowedCityCommand
/// </summary>
public class DeleteAllowedCityCommandValidator : AbstractValidator<DeleteAllowedCityCommand>
{
    public DeleteAllowedCityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID da cidade é obrigatório");
    }
}

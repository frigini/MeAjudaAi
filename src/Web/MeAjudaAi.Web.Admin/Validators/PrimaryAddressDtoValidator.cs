using FluentValidation;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Web.Admin.Extensions;

namespace MeAjudaAi.Web.Admin.Validators;

/// <summary>
/// Validador para PrimaryAddressDto.
/// </summary>
public class PrimaryAddressDtoValidator : AbstractValidator<PrimaryAddressDto>
{
    public PrimaryAddressDtoValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Rua é obrigatória")
            .MinimumLength(3)
            .WithMessage("Rua deve ter no mínimo 3 caracteres")
            .MaximumLength(200)
            .WithMessage("Rua deve ter no máximo 200 caracteres")
            .NoXss();

        When(x => !string.IsNullOrWhiteSpace(x.Number), () =>
        {
            RuleFor(x => x.Number)
                .MaximumLength(20)
                .WithMessage("Número deve ter no máximo 20 caracteres")
                .NoXss();
        });

        When(x => !string.IsNullOrWhiteSpace(x.Complement), () =>
        {
            RuleFor(x => x.Complement)
                .MaximumLength(100)
                .WithMessage("Complemento deve ter no máximo 100 caracteres")
                .NoXss();
        });

        // Bairro é obrigatório
        RuleFor(x => x.Neighborhood)
            .NotEmpty()
            .WithMessage("Bairro é obrigatório")
            .MaximumLength(100)
            .WithMessage("Bairro deve ter no máximo 100 caracteres")
            .NoXss();

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("Cidade é obrigatória")
            .MinimumLength(2)
            .WithMessage("Cidade deve ter no mínimo 2 caracteres")
            .MaximumLength(100)
            .WithMessage("Cidade deve ter no máximo 100 caracteres")
            .NoXss();

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("Estado é obrigatório")
            .Length(2)
            .WithMessage("Estado deve ter 2 caracteres (UF)")
            .Matches(@"^[A-Z]{2}$")
            .WithMessage("Estado deve ser uma UF válida (ex: SP, RJ)");

        RuleFor(x => x.ZipCode)
            .NotEmpty()
            .WithMessage("CEP é obrigatório")
            .ValidCep();

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("País é obrigatório")
            .NoXss()
            .MinimumLength(2)
            .WithMessage("País deve ter no mínimo 2 caracteres")
            .MaximumLength(100)
            .WithMessage("País deve ter no máximo 100 caracteres");
    }
}

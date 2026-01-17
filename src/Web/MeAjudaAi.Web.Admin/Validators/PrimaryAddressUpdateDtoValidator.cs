using FluentValidation;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Web.Admin.Extensions;

namespace MeAjudaAi.Web.Admin.Validators;

/// <summary>
/// Validador para PrimaryAddressUpdateDto.
/// </summary>
public class PrimaryAddressUpdateDtoValidator : AbstractValidator<PrimaryAddressUpdateDto>
{
    public PrimaryAddressUpdateDtoValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.Street), () =>
        {
            RuleFor(x => x.Street)
                .MinimumLength(3)
                .WithMessage("Rua deve ter no mínimo 3 caracteres")
                .MaximumLength(200)
                .WithMessage("Rua deve ter no máximo 200 caracteres")
                .NoXss();
        });

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

        When(x => !string.IsNullOrWhiteSpace(x.Neighborhood), () =>
        {
            RuleFor(x => x.Neighborhood)
                .MaximumLength(100)
                .WithMessage("Bairro deve ter no máximo 100 caracteres")
                .NoXss();
        });

        When(x => !string.IsNullOrWhiteSpace(x.City), () =>
        {
            RuleFor(x => x.City)
                .MinimumLength(2)
                .WithMessage("Cidade deve ter no mínimo 2 caracteres")
                .MaximumLength(100)
                .WithMessage("Cidade deve ter no máximo 100 caracteres")
                .NoXss();
        });

        When(x => !string.IsNullOrWhiteSpace(x.State), () =>
        {
            RuleFor(x => x.State)
                .Length(2)
                .WithMessage("Estado deve ter 2 caracteres (UF)")
                .Matches(@"^[A-Z]{2}$")
                .WithMessage("Estado deve ser uma UF válida (ex: SP, RJ)");
        });

        When(x => !string.IsNullOrWhiteSpace(x.ZipCode), () =>
        {
            RuleFor(x => x.ZipCode)
                .ValidCep();
        });

        When(x => !string.IsNullOrWhiteSpace(x.Country), () =>
        {
            RuleFor(x => x.Country)
                .NoXss()
                .MinimumLength(2)
                .WithMessage("País deve ter no mínimo 2 caracteres")
                .MaximumLength(100)
                .WithMessage("País deve ter no máximo 100 caracteres");
        });
    }
}

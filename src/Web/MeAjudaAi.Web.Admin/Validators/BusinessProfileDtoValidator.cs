using FluentValidation;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Web.Admin.Extensions;

namespace MeAjudaAi.Web.Admin.Validators;

/// <summary>
/// Validador para BusinessProfileDto.
/// </summary>
public class BusinessProfileDtoValidator : AbstractValidator<BusinessProfileDto>
{
    public BusinessProfileDtoValidator()
    {
        RuleFor(x => x.LegalName)
            .NotEmpty()
            .WithMessage("Razão social é obrigatória")
            .MinimumLength(3)
            .WithMessage("Razão social deve ter no mínimo 3 caracteres")
            .MaximumLength(200)
            .WithMessage("Razão social deve ter no máximo 200 caracteres")
            .NoXss();

        When(x => !string.IsNullOrWhiteSpace(x.FantasyName), () =>
        {
            RuleFor(x => x.FantasyName)
                .MinimumLength(3)
                .WithMessage("Nome fantasia deve ter no mínimo 3 caracteres")
                .MaximumLength(200)
                .WithMessage("Nome fantasia deve ter no máximo 200 caracteres")
                .NoXss();
        });

        When(x => !string.IsNullOrWhiteSpace(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Descrição deve ter no máximo 1000 caracteres")
                .NoXss();
        });

        RuleFor(x => x.ContactInfo)
            .NotNull()
            .WithMessage("Informações de contato são obrigatórias")
            .SetValidator(new ContactInfoDtoValidator());

        RuleFor(x => x.PrimaryAddress)
            .NotNull()
            .WithMessage("Endereço é obrigatório")
            .SetValidator(new PrimaryAddressDtoValidator());
    }
}

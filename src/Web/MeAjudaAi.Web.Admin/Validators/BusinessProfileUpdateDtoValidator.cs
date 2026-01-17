using FluentValidation;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Web.Admin.Extensions;

namespace MeAjudaAi.Web.Admin.Validators;

/// <summary>
/// Validador para BusinessProfileUpdateDto.
/// </summary>
public class BusinessProfileUpdateDtoValidator : AbstractValidator<BusinessProfileUpdateDto>
{
    public BusinessProfileUpdateDtoValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.LegalName), () =>
        {
            RuleFor(x => x.LegalName)
                .MinimumLength(3)
                .WithMessage("Razão social deve ter no mínimo 3 caracteres")
                .MaximumLength(200)
                .WithMessage("Razão social deve ter no máximo 200 caracteres")
                .NoXss();
        });

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

        When(x => x.ContactInfo != null, () =>
        {
            RuleFor(x => x.ContactInfo)
                .SetValidator(new ContactInfoUpdateDtoValidator()!);
        });

        When(x => x.PrimaryAddress != null, () =>
        {
            RuleFor(x => x.PrimaryAddress)
                .SetValidator(new PrimaryAddressUpdateDtoValidator()!);
        });
    }
}

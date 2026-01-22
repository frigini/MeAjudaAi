using FluentValidation;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Web.Admin.Extensions;

namespace MeAjudaAi.Web.Admin.Validators;

/// <summary>
/// Validador para UpdateProviderRequestDto com regras de negócio do Brasil.
/// Como todos os campos são opcionais, valida apenas quando informados.
/// </summary>
public class UpdateProviderRequestDtoValidator : AbstractValidator<UpdateProviderRequestDto>
{
    public UpdateProviderRequestDtoValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MinimumLength(3)
                .WithMessage("Nome deve ter no mínimo 3 caracteres")
                .MaximumLength(200)
                .WithMessage("Nome deve ter no máximo 200 caracteres")
                .NoXss()
                .WithMessage("Nome contém caracteres não permitidos");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Phone), () =>
        {
            RuleFor(x => x.Phone)
                .ValidBrazilianPhone()
                .WithMessage("Telefone inválido. Use formato brasileiro: (00) 00000-0000");
        });

        When(x => x.BusinessProfile != null, () =>
        {
            RuleFor(x => x.BusinessProfile)
                .SetValidator(new BusinessProfileUpdateDtoValidator()!);
        });
    }
}

using FluentValidation;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Web.Admin.Extensions;

namespace MeAjudaAi.Web.Admin.Validators;

/// <summary>
/// Validador para CreateProviderRequestDto com regras de negócio do Brasil.
/// </summary>
public class CreateProviderRequestDtoValidator : AbstractValidator<CreateProviderRequestDto>
{
    public CreateProviderRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MinimumLength(3)
            .WithMessage("Nome deve ter no mínimo 3 caracteres")
            .MaximumLength(200)
            .WithMessage("Nome deve ter no máximo 200 caracteres")
            .NoXss()
            .WithMessage("Nome contém caracteres não permitidos");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Tipo de provider inválido");

        // Validação de documento (CPF/CNPJ) - opcional mas deve ser válido se informado
        When(x => !string.IsNullOrWhiteSpace(x.Document), () =>
        {
            RuleFor(x => x.Document)
                .ValidCpfOrCnpj()
                .WithMessage("Documento inválido. Informe um CPF ou CNPJ válido");
        });

        // Validações do BusinessProfile
        RuleFor(x => x.BusinessProfile)
            .NotNull()
            .WithMessage("Perfil de negócio é obrigatório")
            .SetValidator(new BusinessProfileDtoValidator());
    }
}

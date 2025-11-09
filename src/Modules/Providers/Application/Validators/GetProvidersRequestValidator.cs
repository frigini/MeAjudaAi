using FluentValidation;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Constants;

namespace MeAjudaAi.Modules.Providers.Application.Validators;

/// <summary>
/// Validator para GetProvidersRequest
/// </summary>
public class GetProvidersRequestValidator : AbstractValidator<GetProvidersRequest>
{
    public GetProvidersRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Número da página deve ser maior que 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(ValidationConstants.Pagination.MinPageSize - 1)
            .WithMessage("Tamanho da página deve ser maior que 0")
            .LessThanOrEqualTo(ValidationConstants.Pagination.MaxPageSize)
            .WithMessage("Tamanho da página não pode ser maior que 100");

        When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MinimumLength(2)
                .WithMessage("Nome deve ter pelo menos 2 caracteres")
                .MaximumLength(100)
                .WithMessage("Nome não pode ter mais de 100 caracteres");
        });

        When(x => x.Type.HasValue, () =>
        {
            RuleFor(x => x.Type!.Value)
                .Must(type => Enum.IsDefined(typeof(EProviderType), type))
                .WithMessage("Tipo de prestador inválido. Valores válidos: 0 (None), 1 (Individual), 2 (Company), 3 (Cooperative), 4 (Freelancer)");
        });

        When(x => x.VerificationStatus.HasValue, () =>
        {
            RuleFor(x => x.VerificationStatus!.Value)
                .Must(status => Enum.IsDefined(typeof(EVerificationStatus), status))
                .WithMessage("Status de verificação inválido. Valores válidos: 0 (None), 1 (Pending), 2 (InProgress), 3 (Verified), 4 (Rejected), 5 (Suspended)");
        });
    }
}

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
            .GreaterThanOrEqualTo(ValidationConstants.Pagination.MinPageSize)
            .WithMessage($"Tamanho da página deve ser pelo menos {ValidationConstants.Pagination.MinPageSize}")
            .LessThanOrEqualTo(ValidationConstants.Pagination.MaxPageSize)
            .WithMessage($"Tamanho da página não pode ser maior que {ValidationConstants.Pagination.MaxPageSize}");

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
                .WithMessage("Tipo de prestador inválido");
        });

        When(x => x.VerificationStatus.HasValue, () =>
        {
            RuleFor(x => x.VerificationStatus!.Value)
                .Must(status => Enum.IsDefined(typeof(EVerificationStatus), status))
                .WithMessage("Status de verificação inválido");
        });
    }
}

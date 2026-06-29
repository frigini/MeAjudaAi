using FluentValidation;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;

namespace MeAjudaAi.Modules.Users.Application.Validators;

/// <summary>
/// Validator para GetUsersRequest
/// </summary>
public class GetUsersRequestValidator : AbstractValidator<GetUsersRequest>
{
    public GetUsersRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Número da página deve ser maior que 0");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(Pagination.MinPageSize)
            .WithMessage($"Tamanho da página deve ser pelo menos {Pagination.MinPageSize}")
            .LessThanOrEqualTo(Pagination.MaxPageSize)
            .WithMessage($"Tamanho da página não pode ser maior que {Pagination.MaxPageSize}");

        When(x => !string.IsNullOrWhiteSpace(x.SearchTerm), () =>
        {
            RuleFor(x => x.SearchTerm)
                .MinimumLength(2)
                .WithMessage("Termo de busca deve ter pelo menos 2 caracteres")
                .MaximumLength(50)
                .WithMessage("Termo de busca não pode ter mais de 50 caracteres");
        });
    }
}

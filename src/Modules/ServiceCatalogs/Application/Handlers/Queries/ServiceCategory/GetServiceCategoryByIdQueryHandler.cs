using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Mappers;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.Localization;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory;

/// <summary>
/// Handler para processar a consulta GetServiceCategoryByIdQuery, retornando os detalhes de uma categoria de serviço específica com base no ID fornecido.
/// </summary>
/// <param name="queries"></param>
/// <param name="localizer"></param>
public sealed class GetServiceCategoryByIdQueryHandler(
    IServiceCategoryQueries queries,
    IStringLocalizer<Strings> localizer)
    : IQueryHandler<GetServiceCategoryByIdQuery, Result<ServiceCategoryDto?>>
{
    public async Task<Result<ServiceCategoryDto?>> HandleAsync(
        GetServiceCategoryByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        if (request.Id == Guid.Empty)
            return Result<ServiceCategoryDto?>.Failure(localizer["CategoryIdRequired"]);

        var categoryId = ServiceCategoryId.From(request.Id);
        var category = await queries.GetByIdAsync(categoryId, cancellationToken);

        if (category is null)
            return Result<ServiceCategoryDto?>.Success(null);

        var dto = category.ToDto();

        return Result<ServiceCategoryDto?>.Success(dto);
    }
}

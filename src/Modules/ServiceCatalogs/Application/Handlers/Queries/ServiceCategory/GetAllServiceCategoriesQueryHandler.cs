using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Mappings;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory;

/// <summary>
/// Handler para processar a consulta GetAllServiceCategoriesQuery, retornando uma lista de categorias de serviço.
/// </summary>
/// <param name="queries"></param>
public sealed class GetAllServiceCategoriesQueryHandler(IServiceCategoryQueries queries)
    : IQueryHandler<GetAllServiceCategoriesQuery, Result<IReadOnlyList<ServiceCategoryDto>>>
{
    public async Task<Result<IReadOnlyList<ServiceCategoryDto>>> HandleAsync(
        GetAllServiceCategoriesQuery request,
        CancellationToken cancellationToken = default)
    {
        var categories = await queries.GetAllAsync(request.ActiveOnly, cancellationToken);

        var dtos = categories.Select(c => c.ToDto()).ToList();

        return Result<IReadOnlyList<ServiceCategoryDto>>.Success(dtos);
    }
}

using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory;

public sealed class GetServiceCategoriesWithCountQueryHandler(
    IServiceCategoryRepository categoryRepository,
    IServiceRepository serviceRepository)
    : IQueryHandler<GetServiceCategoriesWithCountQuery, Result<IReadOnlyList<ServiceCategoryWithCountDto>>>
{
    public async Task<Result<IReadOnlyList<ServiceCategoryWithCountDto>>> HandleAsync(
        GetServiceCategoriesWithCountQuery request,
        CancellationToken cancellationToken = default)
    {
        var categories = await categoryRepository.GetAllAsync(request.ActiveOnly, cancellationToken);

        var dtos = new List<ServiceCategoryWithCountDto>();

        // NOTA: Isso executa 2 * N consultas de contagem (uma para total, uma para ativo por categoria).
        // Para catálogos pequenos a médios isso é aceitável. Se isso se tornar um gargalo de performance
        // com muitas categorias, considere otimizar com uma consulta em lote ou agrupamento no repositório.
        //
        // DECISÃO (item #4 cleanup): Manter padrão atual.
        // Otimização prematura seria desnecessária sem evidência de problema de performance.
        // Implementação futura deve usar GroupBy em consulta única caso número de categorias cresça significativamente.
        foreach (var category in categories)
        {
            var totalCount = await serviceRepository.CountByCategoryAsync(
                category.Id,
                activeOnly: false,
                cancellationToken);

            var activeCount = await serviceRepository.CountByCategoryAsync(
                category.Id,
                activeOnly: true,
                cancellationToken);

            dtos.Add(new ServiceCategoryWithCountDto(
                category.Id.Value,
                category.Name,
                category.Description,
                category.IsActive,
                category.DisplayOrder,
                activeCount,
                totalCount
            ));
        }

        return Result<IReadOnlyList<ServiceCategoryWithCountDto>>.Success(dtos);
    }
}

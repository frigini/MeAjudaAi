using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Mappings;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service;

public sealed class GetServicesByCategoryQueryHandler(IServiceRepository repository)
    : IQueryHandler<GetServicesByCategoryQuery, Result<IReadOnlyList<ServiceListDto>>>
{
    public async Task<Result<IReadOnlyList<ServiceListDto>>> HandleAsync(
        GetServicesByCategoryQuery request,
        CancellationToken cancellationToken = default)
    {
        if (request.CategoryId == Guid.Empty)
            return Result<IReadOnlyList<ServiceListDto>>.Success(Array.Empty<ServiceListDto>());

        var categoryId = ServiceCategoryId.From(request.CategoryId);
        var services = await repository.GetByCategoryAsync(categoryId, request.ActiveOnly, cancellationToken);

        var dtos = services.Select(s => s.ToListDto()).ToList();

        return Result<IReadOnlyList<ServiceListDto>>.Success(dtos);
    }
}

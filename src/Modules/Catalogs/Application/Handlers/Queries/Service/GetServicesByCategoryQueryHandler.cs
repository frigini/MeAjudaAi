using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Modules.Catalogs.Application.Queries.Service;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Queries.Service;

public sealed class GetServicesByCategoryQueryHandler(IServiceRepository repository)
    : IQueryHandler<GetServicesByCategoryQuery, Result<IReadOnlyList<ServiceListDto>>>
{
    public async Task<Result<IReadOnlyList<ServiceListDto>>> HandleAsync(
        GetServicesByCategoryQuery request,
        CancellationToken cancellationToken = default)
    {
        var categoryId = ServiceCategoryId.From(request.CategoryId);
        var services = await repository.GetByCategoryAsync(categoryId, request.ActiveOnly, cancellationToken);

        var dtos = services.Select(s => new ServiceListDto(
            s.Id.Value,
            s.CategoryId.Value,
            s.Name,
            s.Description,
            s.IsActive
        )).ToList();

        return Result<IReadOnlyList<ServiceListDto>>.Success(dtos);
    }
}

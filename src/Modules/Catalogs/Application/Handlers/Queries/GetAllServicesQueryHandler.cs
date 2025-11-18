using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Modules.Catalogs.Application.Queries;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Queries;

public sealed class GetAllServicesQueryHandler(IServiceRepository repository)
    : IQueryHandler<GetAllServicesQuery, Result<IReadOnlyList<ServiceListDto>>>
{
    public async Task<Result<IReadOnlyList<ServiceListDto>>> HandleAsync(
        GetAllServicesQuery request, 
        CancellationToken cancellationToken = default)
    {
        var services = await repository.GetAllAsync(request.ActiveOnly, cancellationToken);

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

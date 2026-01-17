using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Mappings;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service;

public sealed class GetAllServicesQueryHandler(IServiceRepository repository)
    : IQueryHandler<GetAllServicesQuery, Result<IReadOnlyList<ServiceListDto>>>
{
    public async Task<Result<IReadOnlyList<ServiceListDto>>> HandleAsync(
        GetAllServicesQuery request,
        CancellationToken cancellationToken = default)
    {
        var services = await repository.GetAllAsync(request.ActiveOnly, cancellationToken);

        var dtos = services.Select(s => s.ToListDto()).ToList();

        return Result<IReadOnlyList<ServiceListDto>>.Success(dtos);
    }
}

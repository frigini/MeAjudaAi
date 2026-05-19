using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Mappings;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service;

public sealed class GetAllServicesQueryHandler(IServiceQueries queries)
    : IQueryHandler<GetAllServicesQuery, Result<IReadOnlyList<ServiceListDto>>>
{
    public async Task<Result<IReadOnlyList<ServiceListDto>>> HandleAsync(
        GetAllServicesQuery request,
        CancellationToken cancellationToken = default)
    {
        var services = await queries.GetAllAsync(request.ActiveOnly, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            services = services.Where(s => s.Name.Contains(request.Name, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        var dtos = services.Select(s => s.ToListDto()).ToList();

        return Result<IReadOnlyList<ServiceListDto>>.Success(dtos);
    }
}

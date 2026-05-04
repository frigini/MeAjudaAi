using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Mappings;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using IServiceQueries = MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.IServiceQueries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service;

public sealed class GetAllServicesQueryHandler : IQueryHandler<GetAllServicesQuery, Result<IReadOnlyList<ServiceListDto>>>
{
    private readonly IServiceQueries _queries;

    public GetAllServicesQueryHandler(IServiceQueries queries)
    {
        _queries = queries;
    }

    public async Task<Result<IReadOnlyList<ServiceListDto>>> HandleAsync(
        GetAllServicesQuery request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var services = await _queries.GetAllAsync(request.ActiveOnly, request.Name, cancellationToken);
            
            var dtos = services.Select(s => s.ToListDto()).ToList();
            
            return Result<IReadOnlyList<ServiceListDto>>.Success(dtos);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // Log ex internally if needed, but return generic message
            return Result<IReadOnlyList<ServiceListDto>>.Failure("Erro ao buscar serviços. Tente novamente mais tarde.");
        }
    }
}
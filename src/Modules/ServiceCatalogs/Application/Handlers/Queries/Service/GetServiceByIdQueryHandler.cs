using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Mappings;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service;

public sealed class GetServiceByIdQueryHandler(IServiceQueries serviceQueries)
    : IQueryHandler<GetServiceByIdQuery, Result<ServiceDto?>>
{
    public async Task<Result<ServiceDto?>> HandleAsync(
        GetServiceByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        if (request.Id == Guid.Empty)
            return Result<ServiceDto?>.Failure("O ID do serviço não pode ser vazio.");

        var service = await serviceQueries.GetByIdAsync(ServiceId.From(request.Id), cancellationToken);

        if (service is null)
            return Result<ServiceDto?>.Success(null);

        var dto = service.ToDto();

        return Result<ServiceDto?>.Success(dto);
    }
}
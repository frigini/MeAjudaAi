using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Mappings;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service;

public sealed class GetServiceByIdQueryHandler(IServiceRepository repository)
    : IQueryHandler<GetServiceByIdQuery, Result<ServiceDto?>>
{
    public async Task<Result<ServiceDto?>> HandleAsync(
        GetServiceByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        // Treat Guid.Empty as validation error for consistency with command handlers
        if (request.Id == Guid.Empty)
            return Result<ServiceDto?>.Failure("Service ID cannot be empty.");

        var serviceId = ServiceId.From(request.Id);
        var service = await repository.GetByIdAsync(serviceId, cancellationToken);

        if (service is null)
            return Result<ServiceDto?>.Success(null);

        // Nota: A propriedade de navegação Category deve ser carregada pelo repositório
        var dto = service.ToDto();

        return Result<ServiceDto?>.Success(dto);
    }
}

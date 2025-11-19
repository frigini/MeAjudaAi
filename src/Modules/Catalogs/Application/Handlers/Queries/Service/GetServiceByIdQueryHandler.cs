using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Modules.Catalogs.Application.Queries.Service;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Queries.Service;

public sealed class GetServiceByIdQueryHandler(IServiceRepository repository)
    : IQueryHandler<GetServiceByIdQuery, Result<ServiceDto?>>
{
    public async Task<Result<ServiceDto?>> HandleAsync(
        GetServiceByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        if (request.Id == Guid.Empty)
            return Result<ServiceDto?>.Success(null);

        var serviceId = ServiceId.From(request.Id);
        var service = await repository.GetByIdAsync(serviceId, cancellationToken);

        if (service is null)
            return Result<ServiceDto?>.Success(null);

        // Nota: A propriedade de navegação Category deve ser carregada pelo repositório
        var categoryName = service.Category?.Name ?? "Unknown";

        var dto = new ServiceDto(
            service.Id.Value,
            service.CategoryId.Value,
            categoryName,
            service.Name,
            service.Description,
            service.IsActive,
            service.DisplayOrder,
            service.CreatedAt,
            service.UpdatedAt
        );

        return Result<ServiceDto?>.Success(dto);
    }
}

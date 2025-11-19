using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Constants;
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
        if (request.Id == Guid.Empty)
            return Result<ServiceDto?>.Success(null);

        var serviceId = ServiceId.From(request.Id);
        var service = await repository.GetByIdAsync(serviceId, cancellationToken);

        if (service is null)
            return Result<ServiceDto?>.Success(null);

        // Nota: A propriedade de navegação Category deve ser carregada pelo repositório
        var categoryName = service.Category?.Name ?? ValidationMessages.Catalogs.UnknownCategoryName;

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

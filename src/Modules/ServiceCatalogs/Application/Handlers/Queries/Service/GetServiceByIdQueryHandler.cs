using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Mappers;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.Localization;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service;

/// <summary>
/// Handler para processar a consulta GetServiceByIdQuery, retornando os detalhes de um serviço específico pelo seu ID.
/// </summary>
/// <param name="queries">Consultas de serviço.</param>
/// <param name="localizer">Localizador de mensagens.</param>
public sealed class GetServiceByIdQueryHandler(
    IServiceQueries queries,
    IStringLocalizer<Strings> localizer)
    : IQueryHandler<GetServiceByIdQuery, Result<ServiceDto?>>
{
    public async Task<Result<ServiceDto?>> HandleAsync(
        GetServiceByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        // Trata Guid.Empty como erro de validação para consistência com os command handlers
        if (request.Id == Guid.Empty)
            return Result<ServiceDto?>.Failure(localizer["ServiceIdRequired"]);

        var serviceId = ServiceId.From(request.Id);
        var service = await queries.GetByIdAsync(serviceId, cancellationToken);

        if (service is null)
            return Result<ServiceDto?>.Success(null);

        // Nota: A propriedade de navegação Category deve ser carregada pelo repositório
        var dto = service.ToDto();

        return Result<ServiceDto?>.Success(dto);
    }
}

using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

public sealed class DeleteServiceCommandHandler(
    IServiceRepository serviceRepository)
    : ICommandHandler<DeleteServiceCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteServiceCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Id == Guid.Empty)
            return Result.Failure("Service ID cannot be empty.");

        var serviceId = ServiceId.From(request.Id);
        var service = await serviceRepository.GetByIdAsync(serviceId, cancellationToken);

        if (service is null)
            return Result.Failure($"Service with ID '{request.Id}' not found.");

        // TODO: Verificar se algum provedor oferece este serviço antes de deletar
        // Isso requer integração com o módulo Providers via IProvidersModuleApi
        // Considerar implementar:
        // 1. Chamar IProvidersModuleApi.HasProvidersOfferingServiceAsync(serviceId)
        // 2. Retornar falha se existirem provedores: "Cannot delete service: X providers offer this service"
        // 3. Ou implementar padrão de soft-delete para preservar dados históricos

        await serviceRepository.DeleteAsync(serviceId, cancellationToken);

        return Result.Success();
    }
}

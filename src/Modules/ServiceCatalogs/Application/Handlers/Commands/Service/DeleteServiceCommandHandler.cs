using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

public sealed class DeleteServiceCommandHandler(
    IServiceRepository serviceRepository,
    IProvidersModuleApi providersModuleApi)
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

        // Verificar se algum provedor oferece este serviço antes de deletar
        var hasProvidersResult = await providersModuleApi.HasProvidersOfferingServiceAsync(request.Id, cancellationToken);

        if (hasProvidersResult.IsFailure)
            return Result.Failure($"Failed to verify if providers offer this service: {hasProvidersResult.Error.Message}");

        if (hasProvidersResult.Value)
            return Result.Failure($"Cannot delete service '{service.Name}': it is being offered by one or more providers. Please deactivate the service instead.");

        await serviceRepository.DeleteAsync(serviceId, cancellationToken);

        return Result.Success();
    }
}

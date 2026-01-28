using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

public sealed class DeactivateServiceCommandHandler(
    IServiceRepository serviceRepository)
    : ICommandHandler<DeactivateServiceCommand, Result>
{
    public async Task<Result> HandleAsync(DeactivateServiceCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Id == Guid.Empty)
            return Result.Failure("Service ID cannot be empty.");

        var serviceId = ServiceId.From(request.Id);
        var service = await serviceRepository.GetByIdAsync(serviceId, cancellationToken);

        if (service is null)
            return Result.Failure(Error.NotFound($"Service with ID '{request.Id}' not found."));

        service.Deactivate();

        await serviceRepository.UpdateAsync(service, cancellationToken);

        return Result.Success();
    }
}

using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands;

public sealed class ActivateServiceCommandHandler(
    IServiceRepository serviceRepository) 
    : ICommandHandler<ActivateServiceCommand, Result>
{
    public async Task<Result> HandleAsync(ActivateServiceCommand request, CancellationToken cancellationToken = default)
    {
        var serviceId = ServiceId.From(request.Id);
        var service = await serviceRepository.GetByIdAsync(serviceId, cancellationToken);
        
        if (service is null)
            return Result.Failure($"Service with ID '{request.Id}' not found.");

        service.Activate();
        
        await serviceRepository.UpdateAsync(service, cancellationToken);

        return Result.Success();
    }
}

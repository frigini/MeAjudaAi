using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands;

public sealed class DeleteServiceCommandHandler(
    IServiceRepository serviceRepository) 
    : ICommandHandler<DeleteServiceCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteServiceCommand request, CancellationToken cancellationToken = default)
    {
        var serviceId = ServiceId.From(request.Id);
        var service = await serviceRepository.GetByIdAsync(serviceId, cancellationToken);
        
        if (service is null)
            return Result.Failure($"Service with ID '{request.Id}' not found.");

        // TODO: Check if any provider offers this service before deleting
        // This requires integration with Providers module

        await serviceRepository.DeleteAsync(serviceId, cancellationToken);

        return Result.Success();
    }
}

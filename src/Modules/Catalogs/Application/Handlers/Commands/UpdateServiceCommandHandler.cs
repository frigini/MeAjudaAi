using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Modules.Catalogs.Domain.Exceptions;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands;

public sealed class UpdateServiceCommandHandler(
    IServiceRepository serviceRepository) 
    : ICommandHandler<UpdateServiceCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateServiceCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var serviceId = ServiceId.From(request.Id);
            var service = await serviceRepository.GetByIdAsync(serviceId, cancellationToken);
            
            if (service is null)
                return Result.Failure($"Service with ID '{request.Id}' not found.");

            // Check for duplicate name (excluding current service)
            if (await serviceRepository.ExistsWithNameAsync(request.Name, serviceId, cancellationToken))
                return Result.Failure($"A service with name '{request.Name}' already exists.");

            service.Update(request.Name, request.Description, request.DisplayOrder);
            
            await serviceRepository.UpdateAsync(service, cancellationToken);

            return Result.Success();
        }
        catch (CatalogDomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

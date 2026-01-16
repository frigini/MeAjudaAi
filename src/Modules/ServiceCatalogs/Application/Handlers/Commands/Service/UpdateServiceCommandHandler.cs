using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

public sealed class UpdateServiceCommandHandler(
    IServiceRepository serviceRepository)
    : ICommandHandler<UpdateServiceCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateServiceCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.Id == Guid.Empty)
                return Result.Failure("Service ID cannot be empty.");

            var serviceId = ServiceId.From(request.Id);
            var service = await serviceRepository.GetByIdAsync(serviceId, cancellationToken);

            if (service is null)
                return Result.Failure($"Service with ID '{request.Id}' not found.");

            var normalizedName = request.Name?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
                return Result.Failure("Service name cannot be empty.");

            // Verificar se já existe serviço com o mesmo nome na categoria (excluindo o serviço atual)
            if (await serviceRepository.ExistsWithNameAsync(normalizedName, serviceId, service.CategoryId, cancellationToken))
                return Result.Failure($"A service with name '{normalizedName}' already exists in this category.");

            service.Update(normalizedName, request.Description, request.DisplayOrder);

            await serviceRepository.UpdateAsync(service, cancellationToken);

            return Result.Success();
        }
        catch (CatalogDomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

public sealed class ChangeServiceCategoryCommandHandler(
    IServiceRepository serviceRepository,
    IServiceCategoryRepository categoryRepository)
    : ICommandHandler<ChangeServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(ChangeServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.ServiceId == Guid.Empty)
                return Result.Failure("Service ID cannot be empty.");

            if (request.NewCategoryId == Guid.Empty)
                return Result.Failure("New category ID cannot be empty.");

            var serviceId = ServiceId.From(request.ServiceId);
            var service = await serviceRepository.GetByIdAsync(serviceId, cancellationToken);

            if (service is null)
                return Result.Failure($"Service with ID '{request.ServiceId}' not found.");

            var newCategoryId = ServiceCategoryId.From(request.NewCategoryId);
            var newCategory = await categoryRepository.GetByIdAsync(newCategoryId, cancellationToken);

            if (newCategory is null)
                return Result.Failure($"Category with ID '{request.NewCategoryId}' not found.");

            if (!newCategory.IsActive)
                return Result.Failure("Cannot move service to inactive category.");

            // Garantir que o nome ainda é único na categoria de destino
            if (await serviceRepository.ExistsWithNameAsync(
                    service.Name,
                    service.Id,
                    newCategoryId,
                    cancellationToken))
            {
                return Result.Failure(
                    $"A service with name '{service.Name}' already exists in the target category.");
            }

            service.ChangeCategory(newCategoryId);

            await serviceRepository.UpdateAsync(service, cancellationToken);

            return Result.Success();
        }
        catch (CatalogDomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

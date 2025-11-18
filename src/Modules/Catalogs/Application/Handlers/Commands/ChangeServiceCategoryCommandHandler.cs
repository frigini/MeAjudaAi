using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Modules.Catalogs.Domain.Exceptions;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands;

public sealed class ChangeServiceCategoryCommandHandler(
    IServiceRepository serviceRepository,
    IServiceCategoryRepository categoryRepository) 
    : ICommandHandler<ChangeServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(ChangeServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
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

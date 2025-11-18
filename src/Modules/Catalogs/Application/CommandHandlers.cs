using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.Exceptions;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands;

// ============================================================================
// SERVICE CATEGORY COMMAND HANDLERS
// ============================================================================

public sealed class CreateServiceCategoryCommandHandler(
    IServiceCategoryRepository categoryRepository) 
    : ICommandHandler<CreateServiceCategoryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check for duplicate name
            if (await categoryRepository.ExistsWithNameAsync(request.Name, null, cancellationToken))
                return Result<Guid>.Failure($"A category with name '{request.Name}' already exists.");

            var category = ServiceCategory.Create(request.Name, request.Description, request.DisplayOrder);
            
            await categoryRepository.AddAsync(category, cancellationToken);

            return Result<Guid>.Success(category.Id.Value);
        }
        catch (CatalogDomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

public sealed class UpdateServiceCategoryCommandHandler(
    IServiceCategoryRepository categoryRepository) 
    : ICommandHandler<UpdateServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var categoryId = ServiceCategoryId.From(request.Id);
            var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);
            
            if (category is null)
                return Result.Failure($"Category with ID '{request.Id}' not found.");

            // Check for duplicate name (excluding current category)
            if (await categoryRepository.ExistsWithNameAsync(request.Name, categoryId, cancellationToken))
                return Result.Failure($"A category with name '{request.Name}' already exists.");

            category.Update(request.Name, request.Description, request.DisplayOrder);
            
            await categoryRepository.UpdateAsync(category, cancellationToken);

            return Result.Success();
        }
        catch (CatalogDomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public sealed class DeleteServiceCategoryCommandHandler(
    IServiceCategoryRepository categoryRepository,
    IServiceRepository serviceRepository) 
    : ICommandHandler<DeleteServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        var categoryId = ServiceCategoryId.From(request.Id);
        var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        
        if (category is null)
            return Result.Failure($"Category with ID '{request.Id}' not found.");

        // Check if category has services
        var serviceCount = await serviceRepository.CountByCategoryAsync(categoryId, activeOnly: false, cancellationToken);
        if (serviceCount > 0)
            return Result.Failure($"Cannot delete category with {serviceCount} service(s). Remove or reassign services first.");

        await categoryRepository.DeleteAsync(categoryId, cancellationToken);

        return Result.Success();
    }
}

public sealed class ActivateServiceCategoryCommandHandler(
    IServiceCategoryRepository categoryRepository) 
    : ICommandHandler<ActivateServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(ActivateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        var categoryId = ServiceCategoryId.From(request.Id);
        var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        
        if (category is null)
            return Result.Failure($"Category with ID '{request.Id}' not found.");

        category.Activate();
        
        await categoryRepository.UpdateAsync(category, cancellationToken);

        return Result.Success();
    }
}

public sealed class DeactivateServiceCategoryCommandHandler(
    IServiceCategoryRepository categoryRepository) 
    : ICommandHandler<DeactivateServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(DeactivateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        var categoryId = ServiceCategoryId.From(request.Id);
        var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        
        if (category is null)
            return Result.Failure($"Category with ID '{request.Id}' not found.");

        category.Deactivate();
        
        await categoryRepository.UpdateAsync(category, cancellationToken);

        return Result.Success();
    }
}

// ============================================================================
// SERVICE COMMAND HANDLERS
// ============================================================================

public sealed class CreateServiceCommandHandler(
    IServiceRepository serviceRepository,
    IServiceCategoryRepository categoryRepository) 
    : ICommandHandler<CreateServiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateServiceCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var categoryId = ServiceCategoryId.From(request.CategoryId);
            
            // Verify category exists and is active
            var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);
            if (category is null)
                return Result<Guid>.Failure($"Category with ID '{request.CategoryId}' not found.");
            
            if (!category.IsActive)
                return Result<Guid>.Failure("Cannot create service in inactive category.");

            // Check for duplicate name
            if (await serviceRepository.ExistsWithNameAsync(request.Name, null, cancellationToken))
                return Result<Guid>.Failure($"A service with name '{request.Name}' already exists.");

            var service = Service.Create(categoryId, request.Name, request.Description, request.DisplayOrder);
            
            await serviceRepository.AddAsync(service, cancellationToken);

            return Result<Guid>.Success(service.Id.Value);
        }
        catch (CatalogDomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

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

public sealed class DeactivateServiceCommandHandler(
    IServiceRepository serviceRepository) 
    : ICommandHandler<DeactivateServiceCommand, Result>
{
    public async Task<Result> HandleAsync(DeactivateServiceCommand request, CancellationToken cancellationToken = default)
    {
        var serviceId = ServiceId.From(request.Id);
        var service = await serviceRepository.GetByIdAsync(serviceId, cancellationToken);
        
        if (service is null)
            return Result.Failure($"Service with ID '{request.Id}' not found.");

        service.Deactivate();
        
        await serviceRepository.UpdateAsync(service, cancellationToken);

        return Result.Success();
    }
}

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

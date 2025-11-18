using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.Exceptions;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands;

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

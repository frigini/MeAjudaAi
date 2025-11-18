using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.Exceptions;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands;

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

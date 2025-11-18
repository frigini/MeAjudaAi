using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Modules.Catalogs.Domain.Exceptions;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands;

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

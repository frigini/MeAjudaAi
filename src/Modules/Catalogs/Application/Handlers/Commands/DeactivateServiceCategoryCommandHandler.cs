using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands;

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

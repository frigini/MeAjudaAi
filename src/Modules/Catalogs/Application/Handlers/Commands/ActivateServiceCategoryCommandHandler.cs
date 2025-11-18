using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands;

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

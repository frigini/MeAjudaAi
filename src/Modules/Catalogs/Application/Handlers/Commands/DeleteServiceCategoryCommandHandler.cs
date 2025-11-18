using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands;

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

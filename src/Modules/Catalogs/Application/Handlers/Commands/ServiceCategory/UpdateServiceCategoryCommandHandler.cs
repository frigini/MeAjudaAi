using MeAjudaAi.Modules.Catalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.Catalogs.Domain.Exceptions;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands.ServiceCategory;

public sealed class UpdateServiceCategoryCommandHandler(
    IServiceCategoryRepository categoryRepository)
    : ICommandHandler<UpdateServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.Id == Guid.Empty)
                return Result.Failure("Category ID cannot be empty.");

            var categoryId = ServiceCategoryId.From(request.Id);
            var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);

            if (category is null)
                return Result.Failure($"Category with ID '{request.Id}' not found.");

            var normalizedName = request.Name?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
                return Result.Failure("Category name cannot be empty.");

            // Verificar se já existe categoria com o mesmo nome (excluindo a categoria atual)
            if (await categoryRepository.ExistsWithNameAsync(normalizedName, categoryId, cancellationToken))
                return Result.Failure($"A category with name '{normalizedName}' already exists.");

            category.Update(normalizedName, request.Description, request.DisplayOrder);

            await categoryRepository.UpdateAsync(category, cancellationToken);

            return Result.Success();
        }
        catch (CatalogDomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

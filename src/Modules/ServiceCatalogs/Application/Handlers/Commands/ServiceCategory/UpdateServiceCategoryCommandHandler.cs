using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;

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
                return Result.Failure(Error.NotFound($"Category with ID '{request.Id}' not found."));

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

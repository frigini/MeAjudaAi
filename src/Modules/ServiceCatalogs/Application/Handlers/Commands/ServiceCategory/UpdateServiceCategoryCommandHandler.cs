using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;

public sealed class UpdateServiceCategoryCommandHandler : ICommandHandler<UpdateServiceCategoryCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly IServiceCategoryQueries _categoryQueries;

    public UpdateServiceCategoryCommandHandler(
        [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
        IServiceCategoryQueries categoryQueries)
    {
        _uow = uow;
        _categoryQueries = categoryQueries;
    }

    public async Task<Result> HandleAsync(UpdateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        var uow = _uow;
        var categoryQueries = _categoryQueries;
        try
        {
            if (request.Id == Guid.Empty)
                return Result.Failure(ValidationMessages.Required.Id);

            var categoryId = ServiceCategoryId.From(request.Id);
            var category = await uow.GetRepository<Domain.Entities.ServiceCategory, ServiceCategoryId>().TryFindAsync(categoryId, cancellationToken);

            if (category is null)
                return Result.Failure(Error.NotFound(ValidationMessages.NotFound.Category));

            var normalizedName = request.Name?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
                return Result.Failure(ValidationMessages.Required.CategoryName);

            // Check for duplicate name (excluding current category)
            if (await categoryQueries.ExistsWithNameAsync(normalizedName, categoryId, cancellationToken))
                return Result.Failure(string.Format(ValidationMessages.Catalogs.CategoryNameExists, normalizedName));

            category.Update(normalizedName, request.Description, request.DisplayOrder);

            await uow.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (CatalogDomainException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure($"UNEXPECTED: {ex.Message}");
        }
    }
}

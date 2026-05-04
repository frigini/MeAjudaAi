using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using ServiceCategoryEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;

public sealed class UpdateServiceCategoryCommandHandler(
    IUnitOfWork uow)
    : ICommandHandler<UpdateServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.Id == Guid.Empty)
                return Result.Failure(ValidationMessages.Required.Id);

            var categoryRepository = uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>();
            var category = await categoryRepository.TryFindAsync(ServiceCategoryId.From(request.Id), cancellationToken);

            if (category is null)
                return Result.Failure(Error.NotFound(ValidationMessages.NotFound.Category));

            var normalizedName = request.Name?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
                return Result.Failure(ValidationMessages.Required.CategoryName);

            category.Update(normalizedName, request.Description, request.DisplayOrder);

            await uow.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (CatalogDomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
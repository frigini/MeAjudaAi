using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Functional;
using ValidationMessages = MeAjudaAi.Contracts.Utilities.Constants.ValidationMessages;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Interfaces;
using ServiceCategoryEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;

public sealed class DeactivateServiceCategoryCommandHandler(
    IServiceCatalogUnitOfWork uow)
    : ICommandHandler<DeactivateServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(DeactivateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Id == Guid.Empty)
            return Result.Failure(ValidationMessages.Required.Id);

        var categoryRepository = uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>();
        var category = await categoryRepository.TryFindAsync(ServiceCategoryId.From(request.Id), cancellationToken);

        if (category is null)
            return Result.Failure(Error.NotFound(string.Format(ValidationMessages.NotFound.CategoryById, request.Id)));

        category.Deactivate();
        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
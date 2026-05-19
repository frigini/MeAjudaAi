using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;

public sealed class DeactivateServiceCategoryCommandHandler(
    IUnitOfWork uow)
    : ICommandHandler<DeactivateServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(DeactivateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Id == Guid.Empty)
            return Result.Failure(ValidationMessages.Required.Id);

        var categoryId = ServiceCategoryId.From(request.Id);
        var category = await uow.GetRepository<Domain.Entities.ServiceCategory, ServiceCategoryId>().TryFindAsync(categoryId, cancellationToken);

        if (category is null)
            return Result.Failure(Error.NotFound(string.Format(ValidationMessages.NotFound.CategoryById, request.Id)));

        category.Deactivate();

        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

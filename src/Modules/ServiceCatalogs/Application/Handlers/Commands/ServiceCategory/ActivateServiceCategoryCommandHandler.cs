using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;

public sealed class ActivateServiceCategoryCommandHandler(
    IUnitOfWork uow)
    : ICommandHandler<ActivateServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(ActivateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Id == Guid.Empty)
            return Result.Failure("Category ID cannot be empty.");

        var categoryId = ServiceCategoryId.From(request.Id);
        var category = await uow.GetRepository<Domain.Entities.ServiceCategory, ServiceCategoryId>().TryFindAsync(categoryId, cancellationToken);

        if (category is null)
            return Result.Failure($"Category with ID '{request.Id}' not found.");

        category.Activate();

        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;

public sealed class DeleteServiceCategoryCommandHandler(
    IUnitOfWork uow,
    IServiceQueries serviceQueries)
    : ICommandHandler<DeleteServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Id == Guid.Empty)
            return Result.Failure(ValidationMessages.Required.Id);

        var categoryRepository = uow.GetRepository<ServiceCategory, ServiceCategoryId>();
        var category = await categoryRepository.TryFindAsync(ServiceCategoryId.From(request.Id), cancellationToken);

        if (category is null)
            return Result.Failure(Error.NotFound(ValidationMessages.NotFound.Category));

        // Verificar se existem serviços vinculados
        var serviceCount = await serviceQueries.CountByCategoryAsync(category.Id, false, cancellationToken);
        if (serviceCount > 0)
        {
            return Result.Failure(Error.Conflict(string.Format(ValidationMessages.Catalogs.CannotDeleteCategoryWithServices, serviceCount)));
        }

        categoryRepository.Delete(category);
        await uow.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
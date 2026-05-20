using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;

public sealed class DeleteServiceCategoryCommandHandler : ICommandHandler<DeleteServiceCategoryCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly IServiceQueries _serviceQueries;

    public DeleteServiceCategoryCommandHandler(
        [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
        IServiceQueries serviceQueries)
    {
        _uow = uow;
        _serviceQueries = serviceQueries;
    }

    public async Task<Result> HandleAsync(DeleteServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        var uow = _uow;
        var serviceQueries = _serviceQueries;
        
        if (request.Id == Guid.Empty)
            return Result.Failure(ValidationMessages.Required.Id);

        var categoryId = ServiceCategoryId.From(request.Id);
        var repository = uow.GetRepository<Domain.Entities.ServiceCategory, ServiceCategoryId>();
        var category = await repository.TryFindAsync(categoryId, cancellationToken);

        if (category is null)
            return Result.Failure(Error.NotFound(ValidationMessages.NotFound.Category));

        // Verificar se a categoria possui serviços
        var serviceCount = await serviceQueries.CountByCategoryAsync(categoryId, activeOnly: false, cancellationToken);
        if (serviceCount > 0)
            return Result.Failure(string.Format(ValidationMessages.Catalogs.CannotDeleteCategoryWithServices, serviceCount));

        repository.Delete(category);
        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

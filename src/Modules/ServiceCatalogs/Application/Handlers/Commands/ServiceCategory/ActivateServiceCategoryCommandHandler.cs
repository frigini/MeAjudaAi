using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using ServiceCategoryEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;

public sealed class ActivateServiceCategoryCommandHandler(
    IServiceCatalogUnitOfWork uow)
    : ICommandHandler<ActivateServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(ActivateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Id == Guid.Empty)
            return Result.Failure("O ID da categoria não pode ser vazio.");

        var categoryRepository = uow.GetRepository<ServiceCategoryEntity, ServiceCategoryId>();
        var category = await categoryRepository.TryFindAsync(ServiceCategoryId.From(request.Id), cancellationToken);

        if (category is null)
            return Result.Failure($"Categoria com ID '{request.Id}' não encontrada.");

        category.Activate();
        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
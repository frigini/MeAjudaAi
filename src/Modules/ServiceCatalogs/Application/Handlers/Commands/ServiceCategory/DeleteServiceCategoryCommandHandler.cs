using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;

public sealed class DeleteServiceCategoryCommandHandler(
    IServiceCategoryRepository categoryRepository,
    IServiceRepository serviceRepository)
    : ICommandHandler<DeleteServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Id == Guid.Empty)
            return Result.Failure(ValidationMessages.Required.Id);

        var categoryId = ServiceCategoryId.From(request.Id);
        var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);

        if (category is null)
            return Result.Failure(Error.NotFound(ValidationMessages.NotFound.Category));

        // Verificar se a categoria possui serviços
        var serviceCount = await serviceRepository.CountByCategoryAsync(categoryId, activeOnly: false, cancellationToken);
        if (serviceCount > 0)
            return Result.Failure(string.Format(ValidationMessages.Catalogs.CannotDeleteCategoryWithServices, serviceCount));

        await categoryRepository.DeleteAsync(categoryId, cancellationToken);

        return Result.Success();
    }
}

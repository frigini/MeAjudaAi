using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;

/// <summary>
/// Handler para o comando DeactivateServiceCategoryCommand, responsável por desativar uma categoria de serviço existente no catálogo.
/// </summary>
/// <param name="uow"></param>
/// <param name="logger"></param>
/// <param name="localizer"></param>
public sealed class DeactivateServiceCategoryCommandHandler(
    [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
    ILogger<DeactivateServiceCategoryCommandHandler> logger,
    IStringLocalizer<Strings> localizer) : ICommandHandler<DeactivateServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(DeactivateServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        try
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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while deactivating service category.");
            return Result.Failure(localizer["CategoryDeactivateError"]);
        }
    }
}
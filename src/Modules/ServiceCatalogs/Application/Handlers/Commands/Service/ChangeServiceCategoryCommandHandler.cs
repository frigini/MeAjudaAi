using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

/// <summary>
/// Handler para o comando ChangeServiceCategoryCommand, responsável por alterar a categoria de um serviço existente.
/// </summary>
/// <param name="uow"></param>
/// <param name="serviceQueries"></param>
/// <param name="categoryQueries"></param>
/// <param name="logger"></param>
/// <param name="localizer"></param>
public sealed class ChangeServiceCategoryCommandHandler(
    [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
    IServiceQueries serviceQueries,
    IServiceCategoryQueries categoryQueries,
    ILogger<ChangeServiceCategoryCommandHandler> logger,
    IStringLocalizer<Strings> localizer) : ICommandHandler<ChangeServiceCategoryCommand, Result>
{
    public async Task<Result> HandleAsync(ChangeServiceCategoryCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.ServiceId == Guid.Empty)
                throw new UnprocessableEntityException(localizer["ServiceIdRequired"], "ServiceId");

            if (request.NewCategoryId == Guid.Empty)
                throw new UnprocessableEntityException(localizer["NewCategoryIdRequired"], "NewCategoryId");

            var serviceId = ServiceId.From(request.ServiceId);
            var service = await uow.GetRepository<Domain.Entities.Service, ServiceId>().TryFindAsync(serviceId, cancellationToken);

            if (service is null)
                return Result.Failure(Error.NotFound(localizer["ServiceNotFoundById", request.ServiceId]));

            var newCategoryId = ServiceCategoryId.From(request.NewCategoryId);
            var newCategory = await categoryQueries.GetByIdAsync(newCategoryId, cancellationToken) ?? throw new UnprocessableEntityException(
                    localizer["CategoryNotFoundById", request.NewCategoryId],
                    "ServiceCategory");

            if (!newCategory.IsActive)
                throw new UnprocessableEntityException(
                    localizer["CannotMoveToInactiveCategory"],
                    "ServiceCategory");

            // Garantir que o nome ainda é único na categoria de destino
            if (await serviceQueries.ExistsWithNameAsync(
                    service.Name,
                    service.Id,
                    newCategoryId,
                    cancellationToken))
            {
                return Result.Failure(
                    localizer["ServiceNameExistsInCategory", service.Name]);
            }

            service.ChangeCategory(newCategoryId);

            await uow.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (CatalogDomainException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (UnprocessableEntityException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while changing service category.");
            return Result.Failure(localizer["ServiceCategoryChangeError"]);
        }
    }
}
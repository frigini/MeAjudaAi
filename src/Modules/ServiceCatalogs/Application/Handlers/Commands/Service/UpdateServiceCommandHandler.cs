using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
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
/// Handler para o comando UpdateServiceCommand, responsável por atualizar os detalhes de um serviço existente.
/// </summary>
/// <param name="uow"></param>
/// <param name="serviceQueries"></param>
/// <param name="logger"></param>
/// <param name="localizer"></param>
public sealed class UpdateServiceCommandHandler(
    [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
    IServiceQueries serviceQueries,
    ILogger<UpdateServiceCommandHandler> logger,
    IStringLocalizer<Strings> localizer) : ICommandHandler<UpdateServiceCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateServiceCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.Id == Guid.Empty)
                return Result.Failure(ValidationMessages.Required.Id);

            var serviceId = ServiceId.From(request.Id);
            var service = await uow.GetRepository<Domain.Entities.Service, ServiceId>().TryFindAsync(serviceId, cancellationToken);

            if (service is null)
                return Result.Failure(Error.NotFound(ValidationMessages.NotFound.Service, "NotFound"));

            var normalizedName = request.Name?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
                return Result.Failure(ValidationMessages.Required.ServiceName);

            // Verificar se já existe serviço com o mesmo nome na categoria (excluindo o serviço atual)
            if (await serviceQueries.ExistsWithNameAsync(normalizedName, serviceId, service.CategoryId, cancellationToken))
                return Result.Failure(string.Format(ValidationMessages.Catalogs.ServiceNameExists, normalizedName));

            service.Update(normalizedName, request.Description, request.DisplayOrder);

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
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while updating service.");
            return Result.Failure(localizer["ServiceUpdateError"]);
        }
    }
}
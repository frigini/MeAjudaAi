using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

/// <summary>
/// Handler para o comando DeleteServiceCommand, responsável por deletar um serviço do catálogo.
/// </summary>
/// <param name="uow"></param>
/// <param name="providersModuleApi"></param>
/// <param name="logger"></param>
public sealed class DeleteServiceCommandHandler(
    [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
    IProvidersModuleApi providersModuleApi,
    ILogger<DeleteServiceCommandHandler> logger) : ICommandHandler<DeleteServiceCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteServiceCommand request, CancellationToken cancellationToken = default)
    {
       
        try
        {
            if (request.Id == Guid.Empty)
                return Result.Failure(ValidationMessages.Required.Id);

            var serviceId = ServiceId.From(request.Id);
            var repository = uow.GetRepository<Domain.Entities.Service, ServiceId>();
            var service = await repository.TryFindAsync(serviceId, cancellationToken);

            if (service is null)
                return Result.Failure(Error.NotFound(ValidationMessages.NotFound.Service));

            // Verificar se algum provedor oferece este serviço antes de deletar
            var hasProvidersResult = await providersModuleApi.HasProvidersOfferingServiceAsync(request.Id, cancellationToken);

            if (hasProvidersResult.IsFailure)
                return Result.Failure(hasProvidersResult.Error);

            if (hasProvidersResult.Value)
                return Result.Failure(string.Format(ValidationMessages.Catalogs.CannotDeleteServiceOffered, service.Name));

            repository.Delete(service);
            await uow.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while deleting service.");
            return Result.Failure("Ocorreu um erro inesperado ao excluir o serviço.");
        }
    }
}
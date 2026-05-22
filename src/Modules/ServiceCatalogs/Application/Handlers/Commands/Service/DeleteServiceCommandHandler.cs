using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.DependencyInjection;
using MeAjudaAi.Shared.Database.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

public sealed class DeleteServiceCommandHandler : ICommandHandler<DeleteServiceCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly IProvidersModuleApi _providersModuleApi;
    private readonly ILogger<DeleteServiceCommandHandler> _logger;

    public DeleteServiceCommandHandler(
        [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
        IProvidersModuleApi providersModuleApi,
        ILogger<DeleteServiceCommandHandler> logger)
    {
        _uow = uow;
        _providersModuleApi = providersModuleApi;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(DeleteServiceCommand request, CancellationToken cancellationToken = default)
    {
        var uow = _uow;
        var providersModuleApi = _providersModuleApi;
        
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
            _logger.LogError(ex, "An unexpected error occurred while deleting the service.");
            return Result.Failure("Ocorreu um erro inesperado ao excluir o serviço.");
        }
    }
}

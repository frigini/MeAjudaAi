using MeAjudaAi.Contracts.Functional;
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
/// Handler para o comando DeactivateServiceCommand, responsável por desativar um serviço específico.
/// </summary>
/// <param name="uow"></param>
/// <param name="logger"></param>
public sealed class DeactivateServiceCommandHandler(
    [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
    ILogger<DeactivateServiceCommandHandler> logger) : ICommandHandler<DeactivateServiceCommand, Result>
{
    public async Task<Result> HandleAsync(DeactivateServiceCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.Id == Guid.Empty)
                return Result.Failure(ValidationMessages.Required.Id);

            var serviceId = ServiceId.From(request.Id);
            var service = await uow.GetRepository<Domain.Entities.Service, ServiceId>().TryFindAsync(serviceId, cancellationToken);

            if (service is null)
                return Result.Failure(Error.NotFound(string.Format(ValidationMessages.NotFound.ServiceById, request.Id)));

            service.Deactivate();

            await uow.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while deactivating service.");
            return Result.Failure("Ocorreu um erro inesperado ao desativar o serviço.");
        }
    }
}
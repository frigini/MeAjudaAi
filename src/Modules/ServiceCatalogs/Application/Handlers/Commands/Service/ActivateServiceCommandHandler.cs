using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

/// <summary>
/// Handler para o comando ActivateServiceCommand, responsável por ativar um serviço existente no catálogo.
/// </summary>
public sealed class ActivateServiceCommandHandler(
    [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
    ILogger<ActivateServiceCommandHandler> logger) : ICommandHandler<ActivateServiceCommand, Result>
{
    public async Task<Result> HandleAsync(ActivateServiceCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.Id == Guid.Empty)
                return Result.Failure("O ID do serviço não pode ser vazio.");

            var serviceId = ServiceId.From(request.Id);
            var service = await uow.GetRepository<Domain.Entities.Service, ServiceId>().TryFindAsync(serviceId, cancellationToken);

            if (service is null)
                return Result.Failure(Error.NotFound($"Serviço com ID '{request.Id}' não encontrado."));

            service.Activate();

            await uow.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while activating service.");
            return Result.Failure("Ocorreu um erro inesperado ao ativar o serviço.");
        }
    }
}
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

public sealed class ActivateServiceCommandHandler : ICommandHandler<ActivateServiceCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ActivateServiceCommandHandler> _logger;

    public ActivateServiceCommandHandler(
        [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow,
        ILogger<ActivateServiceCommandHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(ActivateServiceCommand request, CancellationToken cancellationToken = default)
    {
        var uow = _uow;
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
            _logger.LogError(ex, "An unexpected error occurred while activating the service.");
            return Result.Failure("Ocorreu um erro inesperado ao ativar o serviço.");
        }
    }
}

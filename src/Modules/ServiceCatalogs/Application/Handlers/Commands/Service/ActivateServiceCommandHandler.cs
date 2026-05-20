using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

public sealed class ActivateServiceCommandHandler : ICommandHandler<ActivateServiceCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public ActivateServiceCommandHandler(
        [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> HandleAsync(ActivateServiceCommand request, CancellationToken cancellationToken = default)
    {
        var uow = _uow;
        if (request.Id == Guid.Empty)
            return Result.Failure("Service ID cannot be empty.");

        var serviceId = ServiceId.From(request.Id);
        var service = await uow.GetRepository<Domain.Entities.Service, ServiceId>().TryFindAsync(serviceId, cancellationToken);

        if (service is null)
            return Result.Failure($"Service with ID '{request.Id}' not found.");

        service.Activate();

        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

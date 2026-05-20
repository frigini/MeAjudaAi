using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

public sealed class DeactivateServiceCommandHandler : ICommandHandler<DeactivateServiceCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DeactivateServiceCommandHandler(
        [FromKeyedServices(ModuleKeys.ServiceCatalogs)] IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> HandleAsync(DeactivateServiceCommand request, CancellationToken cancellationToken = default)
    {
        var uow = _uow;
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
}

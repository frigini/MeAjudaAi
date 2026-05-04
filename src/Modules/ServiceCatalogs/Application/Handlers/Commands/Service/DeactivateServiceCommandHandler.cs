using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using ValidationMessages = MeAjudaAi.Contracts.Utilities.Constants.ValidationMessages;
using ServiceEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

public sealed class DeactivateServiceCommandHandler(
    IServiceCatalogUnitOfWork uow)
    : ICommandHandler<DeactivateServiceCommand, Result>
{
    public async Task<Result> HandleAsync(DeactivateServiceCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Id == Guid.Empty)
            return Result.Failure(ValidationMessages.Required.Id);

        var serviceRepository = uow.GetRepository<ServiceEntity, ServiceId>();
        var service = await serviceRepository.TryFindAsync(ServiceId.From(request.Id), cancellationToken);

        if (service is null)
            return Result.Failure(Error.NotFound(string.Format(ValidationMessages.NotFound.ServiceById, request.Id)));

        service.Deactivate();
        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
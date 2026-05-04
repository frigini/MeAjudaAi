using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Functional;
using ServiceEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;

public sealed class ActivateServiceCommandHandler(
    IUnitOfWork uow)
    : ICommandHandler<ActivateServiceCommand, Result>
{
    public async Task<Result> HandleAsync(ActivateServiceCommand request, CancellationToken cancellationToken = default)
    {
        if (request.Id == Guid.Empty)
            return Result.Failure("O ID do serviço não pode ser vazio.");

        var serviceRepository = uow.GetRepository<ServiceEntity, ServiceId>();
        var service = await serviceRepository.TryFindAsync(ServiceId.From(request.Id), cancellationToken);

        if (service is null)
            return Result.Failure($"Serviço com ID '{request.Id}' não encontrado.");

        service.Activate();
        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
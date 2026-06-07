using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Locations;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

public sealed class DeleteAllowedCityHandler(
    [FromKeyedServices(ModuleKeys.Locations)] IUnitOfWork uow,
    IMessageBus messageBus,
    ILogger<DeleteAllowedCityHandler> logger) : ICommandHandler<DeleteAllowedCityCommand>
{
    public async Task HandleAsync(DeleteAllowedCityCommand command, CancellationToken cancellationToken = default)
    {
        var repository = uow.GetRepository<AllowedCity, Guid>();
        var city = await repository.TryFindAsync(command.Id, cancellationToken)
            ?? throw new AllowedCityNotFoundException(command.Id);

        repository.Delete(city);
        await uow.SaveChangesAsync(cancellationToken);

        await messageBus.PublishAsync(new AllowedCityDeletedIntegrationEvent(
            ModuleNames.Locations,
            city.Id), cancellationToken: cancellationToken);
            
        logger.LogInformation("AllowedCity {CityId} deleted and event published.", city.Id);
    }
}




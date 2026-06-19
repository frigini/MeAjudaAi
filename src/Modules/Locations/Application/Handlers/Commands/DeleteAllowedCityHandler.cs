using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Application.Handlers.Commands;

public sealed class DeleteAllowedCityHandler(
    [FromKeyedServices(ModuleKeys.Locations)] IUnitOfWork uow,
    ILogger<DeleteAllowedCityHandler> logger) : ICommandHandler<DeleteAllowedCityCommand>
{
    public async Task HandleAsync(DeleteAllowedCityCommand command, CancellationToken cancellationToken = default)
    {
        var repository = uow.GetRepository<AllowedCity, Guid>();
        var city = await repository.TryFindAsync(command.Id, cancellationToken)
            ?? throw new AllowedCityNotFoundException(command.Id);

        repository.Delete(city);
        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("AllowedCity {CityId} deleted.", city.Id);
    }
}
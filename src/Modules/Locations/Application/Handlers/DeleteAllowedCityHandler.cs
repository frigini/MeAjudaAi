using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Commands;

using MeAjudaAi.Modules.Locations.Application.Common;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

public sealed class DeleteAllowedCityHandler(ILocationsUnitOfWork uow) : ICommandHandler<DeleteAllowedCityCommand>
{
    public async Task HandleAsync(DeleteAllowedCityCommand command, CancellationToken cancellationToken = default)
    {
        var repository = uow.GetRepository<AllowedCity, Guid>();
        var city = await repository.TryFindAsync(command.Id, cancellationToken)
            ?? throw new AllowedCityNotFoundException(command.Id);

        repository.Delete(city);
        await uow.SaveChangesAsync(cancellationToken);
    }
}

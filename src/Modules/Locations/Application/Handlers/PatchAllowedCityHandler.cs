using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Extensions;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler para processar atualização parcial de cidade permitida.
/// </summary>
public sealed class PatchAllowedCityHandler(
    IUnitOfWork uow,
    IHttpContextAccessor httpContextAccessor) : ICommandHandler<PatchAllowedCityCommand, Result>
{
    public async Task<Result> HandleAsync(PatchAllowedCityCommand command, CancellationToken cancellationToken = default)
    {
        var repository = uow.GetRepository<AllowedCity, Guid>();
        var allowedCity = await repository.TryFindAsync(command.Id, cancellationToken);
        if (allowedCity == null)
        {
            return Result.Failure(Error.NotFound(ValidationMessages.Locations.AllowedCityNotFound));
        }

        var currentUser = httpContextAccessor.GetAuditIdentity();

        if (!command.ServiceRadiusKm.HasValue && !command.IsActive.HasValue)
        {
            return Result.Success();
        }

        if (command.ServiceRadiusKm.HasValue)
        {
            allowedCity.UpdateRadius(command.ServiceRadiusKm.Value, currentUser);
        }

        if (command.IsActive.HasValue)
        {
            if (command.IsActive.Value)
            {
                allowedCity.Activate(currentUser);
            }
            else
            {
                allowedCity.Deactivate(currentUser);
            }
        }

        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

using MeAjudaAi.Contracts.Utilities.Constants;

using MeAjudaAi.Shared.Extensions;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler para processar atualização parcial de cidade permitida.
/// </summary>
public sealed class PatchAllowedCityHandler(
    IAllowedCityRepository repository,
    IHttpContextAccessor httpContextAccessor) : ICommandHandler<PatchAllowedCityCommand, Result>
{
    public async Task<Result> HandleAsync(PatchAllowedCityCommand command, CancellationToken cancellationToken = default)
    {
        var allowedCity = await repository.GetByIdAsync(command.Id, cancellationToken);
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

        await repository.UpdateAsync(allowedCity, cancellationToken);

        return Result.Success();
    }
}

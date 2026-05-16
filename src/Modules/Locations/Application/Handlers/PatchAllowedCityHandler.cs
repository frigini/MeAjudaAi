using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

using MeAjudaAi.Contracts.Utilities.Constants;


namespace MeAjudaAi.Modules.Locations.Application.Handlers;

public sealed class PatchAllowedCityHandler(
    [FromKeyedServices(ModuleKeys.Locations)] IUnitOfWork uow,
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

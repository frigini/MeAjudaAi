using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Modules.Locations.Domain;
using MeAjudaAi.Shared.Database.Constants;
using Microsoft.Extensions.DependencyInjection;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Locations;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler responsável por processar o comando de atualização de cidade permitida.
/// </summary>
public sealed class UpdateAllowedCityHandler(
    [FromKeyedServices(ModuleKeys.Locations)] IUnitOfWork uow,
    IAllowedCityQueries queries,
    IGeocodingService geocodingService,
    ILogger<UpdateAllowedCityHandler> logger,
    IHttpContextAccessor httpContextAccessor) : ICommandHandler<UpdateAllowedCityCommand, Result>
{
    private const double CoordinateZeroThreshold = 0.0001;

    public async Task<Result> HandleAsync(UpdateAllowedCityCommand command, CancellationToken cancellationToken = default)
    {
        // Obter cidade existente
        var allowedCity = await queries.GetByIdAsync(command.Id, cancellationToken);
        if (allowedCity == null)
        {
            return Result.Failure(Error.NotFound(ValidationMessages.Locations.AllowedCityNotFound));
        }

        // Verificar se novo nome/estado já existe (exceto para esta cidade)
        var existing = await queries.GetByCityAndStateAsync(command.CityName, command.StateSigla, cancellationToken);
        if (existing is not null && existing.Id != command.Id)
        {
            return Result.Failure(Error.Conflict(ValidationMessages.Locations.DuplicateCity));
        }

        // Tentar obter coordenadas se não informadas
        double lat = command.Latitude;
        double lon = command.Longitude;

        // Verificar se precisamos buscar novas coordenadas
        var cityOrStateChanged = !string.Equals(allowedCity.CityName, command.CityName, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(allowedCity.StateSigla, command.StateSigla, StringComparison.OrdinalIgnoreCase);

        var existingCoordsMissing = Math.Abs(allowedCity.Latitude) < CoordinateZeroThreshold && Math.Abs(allowedCity.Longitude) < CoordinateZeroThreshold;
        var commandCoordsMissing = Math.Abs(lat) < CoordinateZeroThreshold && Math.Abs(lon) < CoordinateZeroThreshold;

        if (commandCoordsMissing)
        {
            lat = allowedCity.Latitude;
            lon = allowedCity.Longitude;

            if (cityOrStateChanged || existingCoordsMissing)
            {
                try
                {
                    var address = $"{command.CityName}, {command.StateSigla}, Brasil";
                    var coords = await geocodingService.GetCoordinatesAsync(address, cancellationToken);

                    if (coords != null)
                    {
                        lat = coords.Latitude;
                        lon = coords.Longitude;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Geocoding failed for city {CityName}, {StateSigla}. Keeping existing coordinates.", command.CityName, command.StateSigla);
                }
            }
        }

        var currentUser = httpContextAccessor.GetAuditIdentity();

        allowedCity.Update(
            command.CityName,
            command.StateSigla,
            command.IbgeCode,
            lat,
            lon,
            command.ServiceRadiusKm,
            command.IsActive,
            currentUser);

        await uow.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("AllowedCity {CityId} updated.", allowedCity.Id);

        return Result.Success();
    }
}

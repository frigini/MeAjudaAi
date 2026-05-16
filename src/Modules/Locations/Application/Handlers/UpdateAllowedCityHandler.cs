using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Modules.Locations.Domain;
using MeAjudaAi.Shared.Database.Constants;
using Microsoft.Extensions.DependencyInjection;


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
        // 1. Se cidade ou estado mudaram
        // 2. Ou se as coordenadas atuais são inválidas (0,0)
        // 3. E se o comando não trouxe novas coordenadas válidas (estão zeradas)
        var cityOrStateChanged = !string.Equals(allowedCity.CityName, command.CityName, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(allowedCity.StateSigla, command.StateSigla, StringComparison.OrdinalIgnoreCase);

        var existingCoordsMissing = Math.Abs(allowedCity.Latitude) < CoordinateZeroThreshold && Math.Abs(allowedCity.Longitude) < CoordinateZeroThreshold;
        var commandCoordsMissing = Math.Abs(lat) < CoordinateZeroThreshold && Math.Abs(lon) < CoordinateZeroThreshold;

        if (commandCoordsMissing)
        {
            // Fallback inicial para as coordenadas existentes
            lat = allowedCity.Latitude;
            lon = allowedCity.Longitude;

            // Só tenta geocoding se realmente necessário
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
                    // Manter os valores originais (fallback)
                    logger.LogWarning(ex, "Geocoding failed for city {CityName}, {StateSigla}. Keeping existing coordinates.", command.CityName, command.StateSigla);
                }
            }
        }

        // Obter usuário atual (Admin)
        var currentUser = httpContextAccessor.GetAuditIdentity();

        // Atualizar entidade
        allowedCity.Update(
            command.CityName,
            command.StateSigla,
            command.IbgeCode,
            lat,
            lon,
            command.ServiceRadiusKm,
            command.IsActive,
            currentUser);

        // Persistir - entidade está no tracking do EF via queries
        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

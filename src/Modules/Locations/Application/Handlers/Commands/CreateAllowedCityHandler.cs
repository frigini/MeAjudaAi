using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar o comando de criação de cidade permitida.
/// </summary>
public sealed class CreateAllowedCityHandler(
    [FromKeyedServices(ModuleKeys.Locations)] IUnitOfWork uow,
    IAllowedCityQueries queries,
    IGeocodingService geocodingService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<CreateAllowedCityHandler> logger,
    IStringLocalizer<Strings> localizer) : ICommandHandler<CreateAllowedCityCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateAllowedCityCommand command, CancellationToken cancellationToken = default)
    {
        // Validar se já existe cidade com mesmo nome e estado
        var exists = await queries.ExistsAsync(command.CityName, command.StateSigla, cancellationToken);
        if (exists)
        {
            return Result<Guid>.Failure(Error.Conflict(localizer["CityAlreadyRegistered", $"{command.CityName}-{command.StateSigla}"]));
        }

        // Tentar obter coordenadas se não informadas
        double lat = command.Latitude ?? 0;
        double lon = command.Longitude ?? 0;

        if (!command.Latitude.HasValue && !command.Longitude.HasValue)
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
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex, "Geocoding HTTP request failed for city {CityName}, {StateSigla}. Proceeding with default coordinates.", command.CityName, command.StateSigla);
            }
            catch (TimeoutException ex)
            {
                logger.LogWarning(ex, "Geocoding timed out for city {CityName}, {StateSigla}. Proceeding with default coordinates.", command.CityName, command.StateSigla);
            }
            catch (GeocodingException ex)
            {
                logger.LogWarning(ex, "Geocoding failed for city {CityName}, {StateSigla}. Proceeding with default coordinates.", command.CityName, command.StateSigla);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "Geocoding failed for city {CityName}, {StateSigla}. Proceeding with default coordinates.", command.CityName, command.StateSigla);
            }
        }

        // Obter usuário atual (Admin)
        var currentUser = httpContextAccessor.GetAuditIdentity();

        // Criar entidade
        var allowedCity = new AllowedCity(
            command.CityName,
            command.StateSigla,
            currentUser,
            command.IbgeCode,
            lat,
            lon,
            command.ServiceRadiusKm,
            command.IsActive);

        // Persistir
        uow.GetRepository<AllowedCity, Guid>().Add(allowedCity);
        await uow.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(allowedCity.Id);
    }
}
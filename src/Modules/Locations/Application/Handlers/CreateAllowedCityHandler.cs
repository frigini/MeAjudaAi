using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler responsável por processar o comando de criação de cidade permitida.
/// </summary>
public sealed class CreateAllowedCityHandler(
    IUnitOfWork uow,
    IAllowedCityQueries queries,
    IGeocodingService geocodingService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<CreateAllowedCityHandler> logger) : ICommandHandler<CreateAllowedCityCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateAllowedCityCommand command, CancellationToken cancellationToken = default)
    {
        // Validar se já existe cidade com mesmo nome e estado
        var exists = await queries.ExistsAsync(command.CityName, command.StateSigla, cancellationToken);
        if (exists)
        {
            return Result<Guid>.Failure(Error.Conflict($"Cidade '{command.CityName}-{command.StateSigla}' já cadastrada"));
        }

        // Tentar obter coordenadas se não informadas
        double lat = command.Latitude;
        double lon = command.Longitude;

        if (Math.Abs(lat) < 0.0001 && Math.Abs(lon) < 0.0001)
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
                // Falha no geocoding não é bloqueante; o usuário pode editar as coordenadas manualmente
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

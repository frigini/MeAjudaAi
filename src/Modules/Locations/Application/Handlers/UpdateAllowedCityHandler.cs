using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler responsável por processar o comando de atualização de cidade permitida.
/// </summary>
public sealed class UpdateAllowedCityHandler(
    IAllowedCityRepository repository,
    IGeocodingService geocodingService,
    IHttpContextAccessor httpContextAccessor) : ICommandHandler<UpdateAllowedCityCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateAllowedCityCommand command, CancellationToken cancellationToken = default)
    {
        // Obter cidade existente
        var allowedCity = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (allowedCity == null)
        {
            return Result.Failure(Error.NotFound("Cidade permitida não encontrada"));
        }

        // Verificar se novo nome/estado já existe (exceto para esta cidade)
        var existing = await repository.GetByCityAndStateAsync(command.CityName, command.StateSigla, cancellationToken);
        if (existing is not null && existing.Id != command.Id)
        {
            throw new DuplicateAllowedCityException(command.CityName, command.StateSigla);
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
        
        var existingCoordsMissing = Math.Abs(allowedCity.Latitude) < 0.0001 && Math.Abs(allowedCity.Longitude) < 0.0001;
        var commandCoordsMissing = Math.Abs(lat) < 0.0001 && Math.Abs(lon) < 0.0001;

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
                catch
                {
                    // Ignorar erro e manter os valores originais (fallback)
                }
            }
        }

        // Obter usuário atual (Admin)
        var currentUser = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email) ?? "system";

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

        // Persistir
        await repository.UpdateAsync(allowedCity, cancellationToken);

        return Result.Success();
    }
}

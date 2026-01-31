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

        // Se lat/long forem 0 e o comando indicar que mudou cidade/estado ou é uma correção, tenta geocoding
        // Aqui assumimos que se veio 0, o frontend não enviou, então tentamos obter
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
            catch
            {
                // Ignorar erro
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

using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler responsável por processar o comando de criação de cidade permitida.
/// </summary>
public sealed class CreateAllowedCityHandler(
    IAllowedCityRepository repository,
    IHttpContextAccessor httpContextAccessor) : ICommandHandler<CreateAllowedCityCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreateAllowedCityCommand command, CancellationToken cancellationToken)
    {
        // Validar se já existe cidade com mesmo nome e estado
        var exists = await repository.ExistsAsync(command.CityName, command.StateSigla, cancellationToken);
        if (exists)
        {
            throw new DuplicateAllowedCityException(command.CityName, command.StateSigla);
        }

        // Obter usuário atual (Admin)
        var currentUser = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email) ?? "system";

        // Criar entidade
        var allowedCity = new AllowedCity(
            command.CityName,
            command.StateSigla,
            currentUser,
            command.IbgeCode,
            command.IsActive);

        // Persistir
        await repository.AddAsync(allowedCity, cancellationToken);

        return allowedCity.Id;
    }
}

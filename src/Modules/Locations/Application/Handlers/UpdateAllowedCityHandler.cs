using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler responsável por processar o comando de atualização de cidade permitida.
/// </summary>
public sealed class UpdateAllowedCityHandler(
    IAllowedCityRepository repository,
    IHttpContextAccessor httpContextAccessor) : ICommandHandler<UpdateAllowedCityCommand>
{
    public async Task HandleAsync(UpdateAllowedCityCommand command, CancellationToken cancellationToken)
    {
        // Buscar entidade existente
        var city = await repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new AllowedCityNotFoundException(command.Id);

        // Verificar se novo nome/estado já existe (exceto para esta cidade)
        var existing = await repository.GetByCityAndStateAsync(command.CityName, command.StateSigla, cancellationToken);
        if (existing is not null && existing.Id != command.Id)
        {
            throw new DuplicateAllowedCityException(command.CityName, command.StateSigla);
        }

        // Obter usuário atual (Admin)
        var currentUser = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email) ?? "system";

        // Atualizar entidade
        city.Update(
            command.CityName,
            command.StateSigla,
            command.IbgeCode,
            command.IsActive,
            currentUser);

        // Persistir alterações
        await repository.UpdateAsync(city, cancellationToken);
    }
}

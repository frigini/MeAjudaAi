using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler responsável por processar o comando de exclusão de cidade permitida.
/// </summary>
public sealed class DeleteAllowedCityHandler(IAllowedCityRepository repository) : ICommandHandler<DeleteAllowedCityCommand>
{
    public async Task HandleAsync(DeleteAllowedCityCommand command, CancellationToken cancellationToken = default)
    {
        // Buscar entidade existente
        var city = await repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new AllowedCityNotFoundException(command.Id);

        // Deletar
        await repository.DeleteAsync(city, cancellationToken);
    }
}

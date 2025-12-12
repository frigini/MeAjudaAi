using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler responsável por processar o comando de exclusão de cidade permitida.
/// </summary>
public sealed class DeleteAllowedCityHandler(IAllowedCityRepository repository) : ICommandHandler<DeleteAllowedCityCommand>
{
    public async Task HandleAsync(DeleteAllowedCityCommand command, CancellationToken cancellationToken)
    {
        // Buscar entidade existente
        var city = await repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Cidade com ID '{command.Id}' não encontrada");

        // Deletar
        await repository.DeleteAsync(city, cancellationToken);
    }
}

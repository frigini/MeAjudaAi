using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler responsável por processar o comando de exclusão de cidade permitida.
/// </summary>
public sealed class DeleteAllowedCityHandler(
    IUnitOfWork uow,
    IAllowedCityQueries queries) : ICommandHandler<DeleteAllowedCityCommand>
{
    public async Task HandleAsync(DeleteAllowedCityCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            // Buscar entidade existente
            var city = await queries.GetByIdAsync(command.Id, cancellationToken)
                ?? throw new AllowedCityNotFoundException(command.Id);

            // Deletar
            uow.GetRepository<AllowedCity, Guid>().Delete(city);
            await uow.SaveChangesAsync(cancellationToken);
        }
        catch (AllowedCityNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete allowed city with ID {command.Id}", ex);
        }
    }
}

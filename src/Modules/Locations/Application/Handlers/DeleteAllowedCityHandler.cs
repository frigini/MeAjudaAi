using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using MediatR;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler para deleção de cidade permitida
/// </summary>
internal sealed class DeleteAllowedCityHandler(
    IAllowedCityRepository repository) : IRequestHandler<DeleteAllowedCityCommand, Unit>
{
    public async Task<Unit> Handle(DeleteAllowedCityCommand request, CancellationToken cancellationToken)
    {
        // Buscar entidade existente
        var allowedCity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Cidade permitida com ID '{request.Id}' não encontrada");

        // Deletar
        await repository.DeleteAsync(allowedCity, cancellationToken);

        return Unit.Value;
    }
}

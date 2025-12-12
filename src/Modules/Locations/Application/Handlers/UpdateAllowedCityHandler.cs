using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler para atualização de cidade permitida
/// </summary>
internal sealed class UpdateAllowedCityHandler(
    IAllowedCityRepository repository,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<UpdateAllowedCityCommand, Unit>
{
    public async Task<Unit> Handle(UpdateAllowedCityCommand request, CancellationToken cancellationToken)
    {
        // Buscar entidade existente
        var allowedCity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Cidade permitida com ID '{request.Id}' não encontrada");

        // Verificar se novo nome/estado já existe (exceto para esta cidade)
        var existing = await repository.GetByCityAndStateAsync(request.CityName, request.StateSigla, cancellationToken);
        if (existing is not null && existing.Id != request.Id)
        {
            throw new InvalidOperationException($"Já existe outra cidade '{request.CityName}-{request.StateSigla}' cadastrada");
        }

        // Obter usuário atual (Admin)
        var currentUser = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";

        // Atualizar entidade
        allowedCity.Update(
            request.CityName,
            request.StateSigla,
            request.IbgeCode,
            request.IsActive,
            currentUser);

        // Persistir
        await repository.UpdateAsync(allowedCity, cancellationToken);

        return Unit.Value;
    }
}

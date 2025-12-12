using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Modules.Locations.Application.Handlers;

/// <summary>
/// Handler para criação de cidade permitida
/// </summary>
internal sealed class CreateAllowedCityHandler(
    IAllowedCityRepository repository,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<CreateAllowedCityCommand, Guid>
{
    public async Task<Guid> Handle(CreateAllowedCityCommand request, CancellationToken cancellationToken)
    {
        // Validar se já existe cidade com mesmo nome e estado
        var exists = await repository.ExistsAsync(request.CityName, request.StateSigla, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Cidade '{request.CityName}-{request.StateSigla}' já existe na lista de cidades permitidas");
        }

        // Obter usuário atual (Admin)
        var currentUser = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";

        // Criar entidade
        var allowedCity = new AllowedCity(
            request.CityName,
            request.StateSigla,
            currentUser,
            request.IbgeCode,
            request.IsActive);

        // Persistir
        await repository.AddAsync(allowedCity, cancellationToken);

        return allowedCity.Id;
    }
}

using MeAjudaAi.Modules.Locations.Domain.Entities;

namespace MeAjudaAi.Modules.Locations.Domain.Repositories;

/// <summary>
/// Repositório para gerenciamento de cidades permitidas
/// </summary>
public interface IAllowedCityRepository
{
    /// <summary>
    /// Busca todas as cidades permitidas ativas
    /// </summary>
    Task<IReadOnlyList<AllowedCity>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca todas as cidades permitidas (incluindo inativas)
    /// </summary>
    Task<IReadOnlyList<AllowedCity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca cidade permitida por ID
    /// </summary>
    Task<AllowedCity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca cidade permitida por nome e estado
    /// </summary>
    Task<AllowedCity?> GetByCityAndStateAsync(string cityName, string stateSigla, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se uma cidade está permitida (ativa)
    /// </summary>
    Task<bool> IsCityAllowedAsync(string cityName, string stateSigla, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona nova cidade permitida
    /// </summary>
    Task AddAsync(AllowedCity allowedCity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza cidade permitida existente
    /// </summary>
    Task UpdateAsync(AllowedCity allowedCity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove cidade permitida
    /// </summary>
    Task DeleteAsync(AllowedCity allowedCity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se já existe cidade com mesmo nome e estado
    /// </summary>
    Task<bool> ExistsAsync(string cityName, string stateSigla, CancellationToken cancellationToken = default);
}

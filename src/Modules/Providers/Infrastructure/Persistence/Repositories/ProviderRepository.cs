using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Contracts;
using MeAjudaAi.Contracts.Models;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação do repositório de prestadores de serviços usando Entity Framework Core.
/// </summary>
/// <remarks>
/// Implementa o padrão Repository para operações de persistência de prestadores de serviços,
/// encapsulando a lógica de acesso a dados e mantendo a separação entre domínio e infraestrutura.
/// </remarks>
/// <param name="context">Contexto do Entity Framework</param>
public sealed class ProviderRepository(ProvidersDbContext context) : IProviderRepository
{
    /// <summary>
    /// Base query with common includes for providers.
    /// </summary>
    private IQueryable<Provider> GetProvidersQuery() =>
        context.Providers
            .Include(p => p.Documents)
            .Include(p => p.Qualifications)
            .Include(p => p.Services)
            .AsSplitQuery();

    /// <summary>
    /// Adiciona um novo prestador de serviços ao repositório.
    /// </summary>
    public async Task AddAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        await context.Providers.AddAsync(provider, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Busca um prestador de serviços pelo ID.
    /// </summary>
    public async Task<Provider?> GetByIdAsync(ProviderId id, CancellationToken cancellationToken = default)
    {
        return await GetProvidersQuery()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Busca múltiplos prestadores de serviços pelos seus identificadores únicos.
    /// </summary>
    public async Task<IReadOnlyList<Provider>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default)
    {
        // Convert Guids to ProviderIds for EF Core
        var providerIds = ids.Select(id => new ProviderId(id)).ToList();
        return await GetProvidersQuery()
            .Where(p => providerIds.Contains(p.Id) && !p.IsDeleted)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca um prestador de serviços pelo ID do usuário.
    /// </summary>
    public async Task<Provider?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetProvidersQuery()
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Busca um prestador de serviços por documento (CPF, CNPJ, etc.).
    /// </summary>
    public async Task<Provider?> GetByDocumentAsync(string document, CancellationToken cancellationToken = default)
    {
        return await GetProvidersQuery()
            .FirstOrDefaultAsync(p => p.Documents.Any(d => d.Number == document) && !p.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Verifica se existe um prestador de serviços para o usuário especificado.
    /// </summary>
    public async Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Providers
            .AnyAsync(p => p.UserId == userId && !p.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Busca prestadores de serviços por cidade.
    /// </summary>
    public async Task<IReadOnlyList<Provider>> GetByCityAsync(string city, CancellationToken cancellationToken = default)
    {
        ValidateSearchInput(city, nameof(city));
        return await GetProvidersQuery()
            .Where(p => !p.IsDeleted)
            .Where(p => EF.Functions.ILike(p.BusinessProfile.PrimaryAddress.City, $"%{city}%"))
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }
    /// <summary>
    /// Busca prestadores de serviços por estado.
    /// </summary>
    public async Task<IReadOnlyList<Provider>> GetByStateAsync(string state, CancellationToken cancellationToken = default)
    {
        ValidateSearchInput(state, nameof(state));
        return await GetProvidersQuery()
            .Where(p => !p.IsDeleted)
            .Where(p => EF.Functions.ILike(p.BusinessProfile.PrimaryAddress.State, $"%{state}%"))
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca prestadores de serviços por status de verificação.
    /// </summary>
    public async Task<IReadOnlyList<Provider>> GetByVerificationStatusAsync(
        EVerificationStatus verificationStatus,
        CancellationToken cancellationToken = default)
    {
        return await GetProvidersQuery()
            .Where(p => !p.IsDeleted)
            .Where(p => p.VerificationStatus == verificationStatus)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca prestadores de serviços por tipo.
    /// </summary>
    public async Task<IReadOnlyList<Provider>> GetByTypeAsync(
        EProviderType type,
        CancellationToken cancellationToken = default)
    {
        return await GetProvidersQuery()
            .Where(p => !p.IsDeleted)
            .Where(p => p.Type == type)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Atualiza um prestador de serviços existente.
    /// </summary>
    public async Task UpdateAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        context.Providers.Update(provider);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Busca prestadores de serviços com paginação.
    /// </summary>
    public async Task<PagedResult<Provider>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await context.Providers
            .CountAsync(p => !p.IsDeleted, cancellationToken);

        var providers = await GetProvidersQuery()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Provider>
        {
            Items = providers,
            PageNumber = page,
            PageSize = pageSize,
            TotalItems = totalCount
        };
    }

    /// <summary>
    /// Remove um prestador de serviços do repositório (exclusão física).
    /// </summary>
    public async Task DeleteAsync(ProviderId id, CancellationToken cancellationToken = default)
    {
        var provider = await context.Providers.FindAsync([id], cancellationToken);
        if (provider != null)
        {
            context.Providers.Remove(provider);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Verifica se um prestador de serviços existe no repositório.
    /// </summary>
    public async Task<bool> ExistsAsync(ProviderId id, CancellationToken cancellationToken = default)
    {
        return await context.Providers
            .AnyAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Obtém o status de verificação de um prestador de serviços sem carregar a entidade completa.
    /// </summary>
    public async Task<(bool Exists, EVerificationStatus? Status)> GetProviderStatusAsync(
        ProviderId id,
        CancellationToken cancellationToken = default)
    {
        var result = await context.Providers
            .Where(p => p.Id == id && !p.IsDeleted)
            .Select(p => new { p.VerificationStatus })
            .FirstOrDefaultAsync(cancellationToken);

        return result is null
            ? (false, null)
            : (true, result.VerificationStatus);
    }

    /// <summary>
    /// Verifica se existem prestadores de serviços que oferecem um serviço específico.
    /// </summary>
    public async Task<bool> HasProvidersWithServiceAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await context.Providers
            .Where(p => !p.IsDeleted)
            .AnyAsync(p => p.Services.Any(s => s.ServiceId == serviceId), cancellationToken);
    }

    private static void ValidateSearchInput(string input, string paramName)
    {
        if (input.Contains('%') || input.Contains('_'))
            throw new ArgumentException($"{paramName} cannot contain wildcard characters (% or _)", paramName);
    }
}

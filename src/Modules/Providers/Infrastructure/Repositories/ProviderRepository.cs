using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de prestadores de serviços usando Entity Framework Core.
/// </summary>
/// <remarks>
/// Implementa o padrão Repository para operações de persistência de prestadores de serviços,
/// encapsulando a lógica de acesso a dados e mantendo a separação entre domínio e infraestrutura.
/// </remarks>
/// <param name="context">Contexto do Entity Framework</param>
internal sealed class ProviderRepository(ProvidersDbContext context) : IProviderRepository
{
    /// <summary>
    /// Adiciona um novo prestador de serviços ao repositório.
    /// </summary>
    public async Task<Provider> AddAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        var entry = await context.Providers.AddAsync(provider, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return entry.Entity;
    }

    /// <summary>
    /// Busca um prestador de serviços pelo ID.
    /// </summary>
    public async Task<Provider?> GetByIdAsync(ProviderId id, CancellationToken cancellationToken = default)
    {
        return await context.Providers
            .Include(p => p.Documents)
            .Include(p => p.Qualifications)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Busca um prestador de serviços pelo ID do usuário.
    /// </summary>
    public async Task<Provider?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Providers
            .Include(p => p.Documents)
            .Include(p => p.Qualifications)
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted, cancellationToken);
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
        return await context.Providers
            .Include(p => p.Documents)
            .Include(p => p.Qualifications)
            .Where(p => !p.IsDeleted && p.BusinessProfile.PrimaryAddress.City.ToLower() == city.ToLower())
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca prestadores de serviços por estado.
    /// </summary>
    public async Task<IReadOnlyList<Provider>> GetByStateAsync(string state, CancellationToken cancellationToken = default)
    {
        return await context.Providers
            .Include(p => p.Documents)
            .Include(p => p.Qualifications)
            .Where(p => !p.IsDeleted && p.BusinessProfile.PrimaryAddress.State.ToLower() == state.ToLower())
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca prestadores de serviços por status de verificação.
    /// </summary>
    public async Task<IReadOnlyList<Provider>> GetByVerificationStatusAsync(
        EVerificationStatus verificationStatus, 
        CancellationToken cancellationToken = default)
    {
        return await context.Providers
            .Include(p => p.Documents)
            .Include(p => p.Qualifications)
            .Where(p => !p.IsDeleted && p.VerificationStatus == verificationStatus)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca prestadores de serviços por tipo.
    /// </summary>
    public async Task<IReadOnlyList<Provider>> GetByTypeAsync(
        EProviderType type, 
        CancellationToken cancellationToken = default)
    {
        return await context.Providers
            .Include(p => p.Documents)
            .Include(p => p.Qualifications)
            .Where(p => !p.IsDeleted && p.Type == type)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Atualiza um prestador de serviços existente.
    /// </summary>
    public async Task<Provider> UpdateAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        context.Providers.Update(provider);
        await context.SaveChangesAsync(cancellationToken);
        return provider;
    }

    /// <summary>
    /// Remove um prestador de serviços do repositório.
    /// </summary>
    public async Task DeleteAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        context.Providers.Remove(provider);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Busca todos os prestadores de serviços.
    /// </summary>
    public async Task<IReadOnlyList<Provider>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Providers
            .Include(p => p.Documents)
            .Include(p => p.Qualifications)
            .Where(p => !p.IsDeleted)
            .ToListAsync(cancellationToken);
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

        var providers = await context.Providers
            .Include(p => p.Documents)
            .Include(p => p.Qualifications)
            .Where(p => !p.IsDeleted)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Provider>(
            providers,
            totalCount,
            page,
            pageSize);
    }
}

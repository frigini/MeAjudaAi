using MeAjudaAi.Modules.Providers.Application.Services;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Queries;

/// <summary>
/// Implementação do serviço de consultas de prestadores de serviços.
/// </summary>
/// <remarks>
/// Implementa consultas complexas e paginadas específicas da infraestrutura
/// que não fazem parte do domínio principal.
/// </remarks>
public sealed class ProviderQueryService(ProvidersDbContext context) : IProviderQueryService
{

    /// <summary>
    /// Busca prestadores de serviços com paginação e filtros opcionais.
    /// </summary>
    public async Task<PagedResult<Provider>> GetProvidersAsync(
        int page = 1,
        int pageSize = 20,
        string? nameFilter = null,
        EProviderType? typeFilter = null,
        EVerificationStatus? verificationStatusFilter = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Providers
            .Include(p => p.Documents)
            .Include(p => p.Qualifications)
            .Where(p => !p.IsDeleted);

        // Aplica filtro por nome (busca parcial, case-insensitive)
        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(p => p.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase));
        }

        // Aplica filtro por tipo
        if (typeFilter.HasValue)
        {
            query = query.Where(p => p.Type == typeFilter.Value);
        }

        // Aplica filtro por status de verificação
        if (verificationStatusFilter.HasValue)
        {
            query = query.Where(p => p.VerificationStatus == verificationStatusFilter.Value);
        }

        // Ordena por data de criação (mais recentes primeiro)
        query = query.OrderByDescending(p => p.CreatedAt);

        // Conta total de registros
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplica paginação
        var providers = await query
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
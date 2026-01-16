using MeAjudaAi.Modules.Providers.Application.Services.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Contracts;
using Microsoft.EntityFrameworkCore;

using MeAjudaAi.Contracts.Models;
namespace MeAjudaAi.Modules.Providers.Tests.Infrastructure;

/// <summary>
/// Versão de teste do ProviderQueryService usando construtor tradicional
/// </summary>
public sealed class TestProviderQueryService : IProviderQueryService
{
    private readonly ProvidersDbContext _context;

    public TestProviderQueryService(ProvidersDbContext context)
    {
        _context = context;
    }

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
        // Valida parâmetros de paginação
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than 0");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize must be greater than 0");

        var query = _context.Providers
            .AsNoTracking()
            .Include(p => p.Documents)
            .Include(p => p.Qualifications)
            .Where(p => !p.IsDeleted);

        // Aplica filtro por nome (busca parcial, case-insensitive)
        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{nameFilter}%"));
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

        // Ordena por data de criação (mais recentes primeiro) com ID como tiebreaker para determinismo
        query = query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id);

        // Conta total de registros
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplica paginação
        var providers = await query
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
}

using MeAjudaAi.Modules.Providers.Application.Services.Interfaces;
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
public sealed class ProviderQueryService : IProviderQueryService
{
    private readonly ProvidersDbContext _context;

    public ProviderQueryService(ProvidersDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Busca prestadores de serviços com paginação e filtros opcionais.
    /// </summary>
    /// <remarks>
    /// <para><b>Provedores de banco de dados suportados:</b></para>
    /// <list type="bullet">
    /// <item><description><b>InMemory</b>: Para testes unitários - usa ToLower().Contains() para compatibilidade</description></item>
    /// <item><description><b>PostgreSQL (Npgsql)</b>: Para produção - usa EF.Functions.ILike() para melhor performance com índices</description></item>
    /// </list>
    /// <para>ILike é específico do PostgreSQL e permite buscas case-insensitive otimizadas com suporte a índices.</para>
    /// </remarks>
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
            .Include(p => p.Documents)
            .Include(p => p.Qualifications)
            .Where(p => !p.IsDeleted);

        // Aplica filtro por nome (busca parcial, case-insensitive)
        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            var providerName = _context.Database.ProviderName;
            
            // Detecta explicitamente o provider de banco de dados
            if (providerName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                // InMemory: usa ToLower() para compatibilidade com testes unitários
                // Nota: Contains() não interpreta wildcards LIKE, então não precisa escapar
                var lowerNameFilter = nameFilter.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(lowerNameFilter));
            }
            else if (providerName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true ||
                     providerName?.Contains("Postgres", StringComparison.OrdinalIgnoreCase) == true)
            {
                // PostgreSQL: usa ILike para melhor performance com índices
                // Escapa caracteres especiais do LIKE (%, _, \) para evitar matches inesperados
                var escapedFilter = nameFilter
                    .Replace("\\", "\\\\")  // Escape backslash first
                    .Replace("%", "\\%")     // Escape percent wildcard
                    .Replace("_", "\\_");    // Escape underscore wildcard
                
                query = query.Where(p => EF.Functions.ILike(p.Name, $"%{escapedFilter}%"));
            }
            else
            {
                throw new NotSupportedException(
                    $"O provedor de banco de dados '{providerName}' não é suportado. " +
                    "Apenas InMemory (para testes) e PostgreSQL/Npgsql (para produção) são suportados.");
            }
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

        return new PagedResult<Provider>(
            providers,
            page,
            pageSize,
            totalCount);
    }
}

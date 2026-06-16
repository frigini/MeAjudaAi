using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Bookings.Application.Queries;

/// <summary>
/// Query para recuperar bookings de um prestador de serviços, com paginação e filtros de data.
/// </summary>
/// <remarks>
/// <para>
/// Esta query implementa <see cref="ICacheableQuery"/> com expiração de 5 minutos e
/// tags de cache para invalidação por <c>CacheTags.Bookings</c> e por prestador específico.
/// </para>
/// <para>
/// Os filtros <c>From</c> e <c>To</c> são opcionais e delimitam o intervalo de datas
/// dos bookings retornados. A paginação é controlada por <c>Page</c> (mínimo 1) e
/// <c>PageSize</c> (mínimo 1, máximo 100).
/// </para>
/// </remarks>
/// <param name="ProviderId">Identificador do prestador cujos bookings serão consultados.</param>
/// <param name="CorrelationId">Identificador de correlação para rastreamento da requisição.</param>
/// <param name="Page">Número da página (mínimo 1). Padrão: 1.</param>
/// <param name="PageSize">Quantidade de itens por página (1–100). Padrão: 10.</param>
/// <param name="From">Data inicial opcional para filtrar bookings (inclusive).</param>
/// <param name="To">Data final opcional para filtrar bookings (inclusive).</param>
/// <returns>
/// Um <see cref="Result{PagedResult}"/> contendo a lista paginada de <see cref="ModuleBookingDto"/>
/// em caso de sucesso, ou um <see cref="Error"/> descritivo em caso de falha.
/// </returns>
public record GetBookingsByProviderQuery(
    Guid ProviderId,
    Guid CorrelationId,
    int Page = 1,
    int PageSize = 10,
    DateTime? From = null,
    DateTime? To = null) : IQuery<Result<PagedResult<ModuleBookingDto>>>, ICacheableQuery
{
    public string GetCacheKey() => $"bookings-provider:{ProviderId}:{Page}:{PageSize}:{From}:{To}";
    public TimeSpan GetCacheExpiration() => TimeSpan.FromMinutes(5);
    public IReadOnlyCollection<string>? GetCacheTags() => 
        [CacheTags.Bookings, CacheTags.ProviderBookingsTag(ProviderId)];
}

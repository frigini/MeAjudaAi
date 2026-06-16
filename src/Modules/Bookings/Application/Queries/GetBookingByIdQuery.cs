using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Bookings.Application.Queries;

/// <summary>
/// Query para obter um booking por ID, com controle de autorização.
/// </summary>
/// <remarks>
/// <para>
/// A autorização é verificada conforme as regras:
/// </para>
/// <list type="bullet">
///   <item>Se <c>IsSystemAdmin</c> é <c>true</c>, acesso total.</item>
///   <item>Se <c>UserId</c> corresponde ao cliente do booking, acesso permitido.</item>
///   <item>Se <c>ProviderId</c> corresponde ao prestador do booking, acesso permitido.</item>
///   <item>Caso contrário, acesso negado.</item>
/// </list>
/// <para>
/// Esta query implementa <see cref="ICacheableQuery"/> com expiração de 15 minutos e
/// tags de cache para invalidação por <c>CacheTags.Bookings</c> e por booking específico.
/// </para>
/// </remarks>
/// <param name="BookingId">Identificador do booking a ser consultado.</param>
/// <param name="UserId">Identificador do usuário autenticado (opcional,用于 autorização de cliente).</param>
/// <param name="ProviderId">Identificador do prestador autenticado (opcional,用于 autorização de prestador).</param>
/// <param name="IsSystemAdmin">Indica se o usuário é administrador do sistema.</param>
/// <param name="CorrelationId">Identificador de correlação para rastreamento da requisição.</param>
/// <returns>
/// Um <see cref="Result{ModuleBookingDto}"/> contendo o booking consultado em caso de sucesso,
/// ou um <see cref="Error"/> descritivo em caso de falha ou acesso negado.
/// </returns>
public record GetBookingByIdQuery(
    Guid BookingId,
    Guid? UserId,
    Guid? ProviderId,
    bool IsSystemAdmin,
    Guid CorrelationId) : IQuery<Result<ModuleBookingDto>>, ICacheableQuery
{
    public string GetCacheKey() => $"booking:{BookingId}";
    public TimeSpan GetCacheExpiration() => TimeSpan.FromMinutes(15);
    public IReadOnlyCollection<string>? GetCacheTags() => 
        [CacheTags.Bookings, CacheTags.BookingTag(BookingId)];
}

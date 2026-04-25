using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Contracts.Bookings.Enums;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.Bookings.Domain.Repositories;

public interface IBookingRepository
{
    /// <summary>
    /// Obtém um agendamento por ID sem rastreamento de mudanças.
    /// </summary>
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um agendamento por ID com rastreamento de mudanças para atualizações.
    /// </summary>
    Task<Booking?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lista agendamentos por prestador com paginação e filtro de data (inclusivo).
    /// </summary>
    Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetByProviderIdPagedAsync(Guid providerId, DateOnly? fromDate, DateOnly? toDate, int page, int pageSize, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lista agendamentos por cliente com paginação e filtro de data (inclusivo).
    /// </summary>
    Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetByClientIdPagedAsync(Guid clientId, DateOnly? fromDate, DateOnly? toDate, int page, int pageSize, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém agendamentos ativos (não cancelados, rejeitados ou concluídos) para uma data.
    /// </summary>
    Task<IReadOnlyList<Booking>> GetActiveByProviderAndDateAsync(Guid providerId, DateOnly date, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adiciona um agendamento garantindo que não há sobreposição de forma atômica.
    /// </summary>
    Task<Result> AddIfNoOverlapAsync(Booking booking, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Atualiza um agendamento existente e trata conflitos de concorrência.
    /// </summary>
    Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default);
}

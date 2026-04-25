using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Contracts.Bookings.Enums;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.Bookings.Domain.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetByProviderIdPagedAsync(Guid providerId, DateOnly? from, DateOnly? to, int page, int pageSize, CancellationToken cancellationToken = default);
    
    Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetByClientIdPagedAsync(Guid clientId, DateOnly? from, DateOnly? to, int page, int pageSize, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Booking>> GetActiveByProviderAndDateAsync(Guid providerId, DateOnly date, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adiciona um agendamento garantindo que não há sobreposição de forma atômica.
    /// </summary>
    Task<Result> AddIfNoOverlapAsync(Booking booking, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default);
}

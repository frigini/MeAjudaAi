using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Contracts.Bookings.Enums;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.Bookings.Domain.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Booking>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Booking>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Booking>> GetByProviderAndStatusAsync(Guid providerId, EBookingStatus status, CancellationToken cancellationToken = default);
    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adiciona um agendamento garantindo que não há sobreposição de forma atômica.
    /// </summary>
    Task<Result> AddIfNoOverlapAsync(Booking booking, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default);
}

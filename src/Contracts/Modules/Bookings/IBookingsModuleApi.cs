using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;

namespace MeAjudaAi.Contracts.Modules.Bookings;

/// <summary>
/// API pública do módulo Bookings para consumo por outros módulos.
/// </summary>
public interface IBookingsModuleApi : IModuleApi
{
    /// <summary>
    /// Obtém detalhes de um agendamento.
    /// </summary>
    Task<Result<BookingDto?>> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe um agendamento concluído entre um cliente e um prestador.
    /// Útil para o módulo de Ratings permitir avaliações.
    /// </summary>
    Task<Result<bool>> HasCompletedBookingAsync(Guid clientId, Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém os agendamentos de um prestador em um período.
    /// </summary>
    Task<Result<IReadOnlyList<BookingDto>>> GetProviderBookingsAsync(Guid providerId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default);
}

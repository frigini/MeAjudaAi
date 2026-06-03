using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using Refit;

namespace MeAjudaAi.Client.Contracts.Api;

public interface IBookingsApi
{
    [Post("/api/v1/bookings")]
    Task<BookingDto> CreateBookingAsync([Body] CreateBookingRequestDto request, CancellationToken cancellationToken = default);

    [Get("/api/v1/bookings/{id}")]
    Task<BookingDto> GetBookingByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

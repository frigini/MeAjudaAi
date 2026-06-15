using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using Refit;

namespace MeAjudaAi.Client.Contracts.Api;

public interface IBookingsApi
{
    [Post("/api/v1/bookings")]
    Task<ModuleBookingDto> CreateBookingAsync(
        [Body] CreateBookingRequestDto request,
        CancellationToken cancellationToken = default);

    [Get("/api/v1/bookings/{id}")]
    Task<ModuleBookingDto> GetBookingByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    [Get("/api/v1/bookings/my")]
    Task<PagedResult<ModuleBookingDto>> GetMyBookingsAsync(
        int? page = null,
        int? pageSize = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default);

    [Get("/api/v1/bookings/provider/{providerId}")]
    Task<PagedResult<ModuleBookingDto>> GetProviderBookingsAsync(
        Guid providerId,
        int? page = null,
        int? pageSize = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);

    [Get("/api/v1/bookings/availability/{providerId}")]
    Task<AvailabilityDto> GetProviderAvailabilityAsync(
        Guid providerId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    [Post("/api/v1/bookings/schedule")]
    Task SetProviderScheduleAsync(
        [Body] SetProviderScheduleRequestDto request,
        CancellationToken cancellationToken = default);

    [Put("/api/v1/bookings/{id}/confirm")]
    Task ConfirmBookingAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    [Put("/api/v1/bookings/{id}/reject")]
    Task RejectBookingAsync(
        Guid id,
        [Body] RejectBookingRequestDto request,
        CancellationToken cancellationToken = default);

    [Put("/api/v1/bookings/{id}/complete")]
    Task CompleteBookingAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    [Put("/api/v1/bookings/{id}/cancel")]
    Task CancelBookingAsync(
        Guid id,
        [Body] CancelBookingRequestDto request,
        CancellationToken cancellationToken = default);

    // SSE endpoint (GET /api/v1/bookings/{id}/events) não é suportado via Refit.
    // Utilizar HttpClient diretamente para streams Server-Sent Events.
}

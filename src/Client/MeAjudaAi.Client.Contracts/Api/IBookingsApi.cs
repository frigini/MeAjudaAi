using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using Refit;

namespace MeAjudaAi.Client.Contracts.Api;

public interface IBookingsApi
{
    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Bookings.Base}")]
    Task<ModuleBookingDto> CreateBookingAsync(
        [Body] CreateBookingRequestDto request,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Bookings.Base}{ApiEndpoints.Bookings.GetById}")]
    Task<ModuleBookingDto> GetBookingByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Bookings.Base}{ApiEndpoints.Bookings.GetMy}")]
    Task<PagedResult<ModuleBookingDto>> GetMyBookingsAsync(
        int? page = null,
        int? pageSize = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Bookings.Base}{ApiEndpoints.Bookings.GetProviderBookings}")]
    Task<PagedResult<ModuleBookingDto>> GetProviderBookingsAsync(
        Guid providerId,
        int? page = null,
        int? pageSize = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Bookings.Base}{ApiEndpoints.Bookings.GetProviderAvailability}")]
    Task<AvailabilityDto> GetProviderAvailabilityAsync(
        Guid providerId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Bookings.Base}{ApiEndpoints.Bookings.SetProviderSchedule}")]
    Task SetProviderScheduleAsync(
        [Body] SetProviderScheduleRequestDto request,
        CancellationToken cancellationToken = default);

    [Put($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Bookings.Base}{ApiEndpoints.Bookings.Confirm}")]
    Task ConfirmBookingAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    [Put($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Bookings.Base}{ApiEndpoints.Bookings.Reject}")]
    Task RejectBookingAsync(
        Guid id,
        [Body] RejectBookingRequestDto request,
        CancellationToken cancellationToken = default);

    [Put($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Bookings.Base}{ApiEndpoints.Bookings.Complete}")]
    Task CompleteBookingAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    [Put($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Bookings.Base}{ApiEndpoints.Bookings.Cancel}")]
    Task CancelBookingAsync(
        Guid id,
        [Body] CancelBookingRequestDto request,
        CancellationToken cancellationToken = default);

    // SSE endpoint (GET /api/v1/bookings/{id}/events) não é suportado via Refit.
    // Utilizar HttpClient diretamente para streams Server-Sent Events.
}
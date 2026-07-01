using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Services;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;

namespace MeAjudaAi.Modules.Bookings.Application.Handlers.Queries;

/// <summary>
/// Handler para processar consultas de booking por ID.
/// </summary>
public sealed class GetBookingByIdQueryHandler(
    IBookingQueries bookingQueries,
    IProviderScheduleQueries scheduleQueries,
    ILogger<GetBookingByIdQueryHandler> logger,
    IStringLocalizer<Strings> localizer) : IQueryHandler<GetBookingByIdQuery, Result<ModuleBookingDto>>
{
    public async Task<Result<ModuleBookingDto>> HandleAsync(GetBookingByIdQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting booking {BookingId}", query.BookingId);

        var booking = await bookingQueries.GetByIdAsync(query.BookingId, cancellationToken);
        if (booking == null)
        {
            return Result<ModuleBookingDto>.Failure(Error.NotFound(localizer["BookingNotFound"]));
        }

        var isAuthorized = query.IsSystemAdmin || 
                           (query.UserId.HasValue && booking.ClientId == query.UserId.Value) ||
                           (query.ProviderId.HasValue && booking.ProviderId == query.ProviderId.Value);

        if (!isAuthorized)
        {
            logger.LogWarning("Unauthorized access attempt to booking {BookingId} by User {UserId} or Provider {ProviderId}", 
                query.BookingId, query.UserId, query.ProviderId);
            return Result<ModuleBookingDto>.Failure(Error.NotFound(localizer["BookingNotFound"]));
        }

        var schedule = await scheduleQueries.GetByProviderIdReadOnlyAsync(booking.ProviderId, cancellationToken);
        var tz = TimeZoneResolver.ResolveTimeZone(schedule?.TimeZoneId, logger);

        if (tz == null)
        {
            logger.LogError("Could not resolve time zone for provider {ProviderId} (Booking {BookingId})", booking.ProviderId, booking.Id);
            return Result<ModuleBookingDto>.Failure(Error.Internal(localizer["ProviderScheduleLoadError"]));
        }

        return TimeZoneResolver.CreateValidatedBookingDto(booking, tz, logger);
    }
}

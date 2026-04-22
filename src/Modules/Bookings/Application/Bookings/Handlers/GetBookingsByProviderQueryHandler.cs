using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class GetBookingsByProviderQueryHandler(
    IBookingRepository bookingRepository,
    IProviderScheduleRepository scheduleRepository,
    ILogger<GetBookingsByProviderQueryHandler> logger) : IQueryHandler<GetBookingsByProviderQuery, Result<IReadOnlyList<BookingDto>>>
{
    public async Task<Result<IReadOnlyList<BookingDto>>> HandleAsync(GetBookingsByProviderQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting bookings for provider {ProviderId}", query.ProviderId);

        var bookings = await bookingRepository.GetByProviderIdAsync(query.ProviderId, cancellationToken);

        var schedule = await scheduleRepository.GetByProviderIdAsync(query.ProviderId, cancellationToken);
        var tz = ResolveTimeZone(schedule?.TimeZoneId);

        var dtos = bookings.Select(booking =>
        {
            var startDate = booking.Date.ToDateTime(booking.TimeSlot.Start);
            var endDate = booking.Date.ToDateTime(booking.TimeSlot.End);

            return new BookingDto(
                booking.Id,
                booking.ProviderId,
                booking.ClientId,
                booking.ServiceId,
                new DateTimeOffset(startDate, tz.GetUtcOffset(startDate)),
                new DateTimeOffset(endDate, tz.GetUtcOffset(endDate)),
                booking.Status,
                booking.RejectionReason,
                booking.CancellationReason);
        }).ToList();

        return Result<IReadOnlyList<BookingDto>>.Success(dtos.AsReadOnly());
    }

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch
        {
            return TimeZoneInfo.Utc;
        }
    }
}

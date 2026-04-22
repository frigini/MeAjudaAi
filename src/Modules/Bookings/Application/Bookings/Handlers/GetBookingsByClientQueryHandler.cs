using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class GetBookingsByClientQueryHandler(
    IBookingRepository bookingRepository,
    IProviderScheduleRepository scheduleRepository,
    ILogger<GetBookingsByClientQueryHandler> logger) : IQueryHandler<GetBookingsByClientQuery, Result<IReadOnlyList<BookingDto>>>
{
    public async Task<Result<IReadOnlyList<BookingDto>>> HandleAsync(GetBookingsByClientQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting bookings for client {ClientId}", query.ClientId);

        var bookings = await bookingRepository.GetByClientIdAsync(query.ClientId, cancellationToken);

        var dtos = new List<BookingDto>();
        foreach (var booking in bookings)
        {
            var schedule = await scheduleRepository.GetByProviderIdAsync(booking.ProviderId, cancellationToken);
            var tz = ResolveTimeZone(schedule?.TimeZoneId);

            var startDate = booking.Date.ToDateTime(booking.TimeSlot.Start);
            var endDate = booking.Date.ToDateTime(booking.TimeSlot.End);

            dtos.Add(new BookingDto(
                booking.Id,
                booking.ProviderId,
                booking.ClientId,
                booking.ServiceId,
                new DateTimeOffset(startDate, tz.GetUtcOffset(startDate)),
                new DateTimeOffset(endDate, tz.GetUtcOffset(endDate)),
                booking.Status,
                booking.RejectionReason,
                booking.CancellationReason));
        }

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

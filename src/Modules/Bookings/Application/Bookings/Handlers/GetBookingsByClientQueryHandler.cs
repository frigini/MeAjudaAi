using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class GetBookingsByClientQueryHandler(
    IBookingRepository bookingRepository,
    IProviderScheduleRepository scheduleRepository,
    ILogger<GetBookingsByClientQueryHandler> logger) : IQueryHandler<GetBookingsByClientQuery, Result<PagedResult<BookingDto>>>
{
    public async Task<Result<PagedResult<BookingDto>>> HandleAsync(GetBookingsByClientQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting bookings for client {ClientId}", query.ClientId);

        var bookings = await bookingRepository.GetByClientIdAsync(query.ClientId, cancellationToken);

        // Apply Date Filters
        if (query.From.HasValue)
        {
            var fromDate = DateOnly.FromDateTime(query.From.Value);
            bookings = bookings.Where(b => b.Date >= fromDate).ToList();
        }

        if (query.To.HasValue)
        {
            var toDate = DateOnly.FromDateTime(query.To.Value);
            bookings = bookings.Where(b => b.Date <= toDate).ToList();
        }

        var totalItems = bookings.Count;

        // Apply Pagination
        var pageNumber = query.Page ?? 1;
        var pageSize = query.PageSize ?? 10;
        var pagedBookings = bookings.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        var dtos = new List<BookingDto>();
        var scheduleCache = new Dictionary<Guid, Domain.Entities.ProviderSchedule?>();

        foreach (var booking in pagedBookings)
        {
            if (!scheduleCache.TryGetValue(booking.ProviderId, out var schedule))
            {
                schedule = await scheduleRepository.GetByProviderIdAsync(booking.ProviderId, cancellationToken);
                scheduleCache[booking.ProviderId] = schedule;
            }

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

        return Result<PagedResult<BookingDto>>.Success(new PagedResult<BookingDto>
        {
            Items = dtos.AsReadOnly(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        });
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

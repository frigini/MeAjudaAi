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

        var pageNumber = query.Page ?? 1;
        var pageSize = query.PageSize ?? 10;
        var fromDate = query.From.HasValue ? DateOnly.FromDateTime(query.From.Value) : (DateOnly?)null;
        var toDate = query.To.HasValue ? DateOnly.FromDateTime(query.To.Value) : (DateOnly?)null;

        var (bookings, totalCount) = await bookingRepository.GetByClientIdPagedAsync(
            query.ClientId,
            fromDate,
            toDate,
            pageNumber,
            pageSize,
            cancellationToken);

        var dtos = new List<BookingDto>();
        var scheduleCache = new Dictionary<Guid, Domain.Entities.ProviderSchedule?>();

        foreach (var booking in bookings)
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
                TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(startDate, tz), tz),
                TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(endDate, tz), tz),
                booking.Status,
                booking.RejectionReason,
                booking.CancellationReason));
        }

        return Result<PagedResult<BookingDto>>.Success(new PagedResult<BookingDto>
        {
            Items = dtos.AsReadOnly(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalCount
        });
    }

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch
            {
                // Ignora e tenta fallback
            }
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
        catch
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }
    }
}

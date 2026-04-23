using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Application.Common;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class GetProviderAvailabilityQueryHandler(
    IBookingRepository bookingRepository,
    IProviderScheduleRepository scheduleRepository,
    ILogger<GetProviderAvailabilityQueryHandler> logger) : IQueryHandler<GetProviderAvailabilityQuery, Result<AvailabilityDto>>
{
    public async Task<Result<AvailabilityDto>> HandleAsync(GetProviderAvailabilityQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting availability for Provider {ProviderId} on {Date}", 
            query.ProviderId, query.Date.ToShortDateString());

        var schedule = await scheduleRepository.GetByProviderIdReadOnlyAsync(query.ProviderId, cancellationToken);
        if (schedule == null)
        {
            return Result<AvailabilityDto>.Failure(Error.NotFound("Agenda do prestador não encontrada."));
        }

        var daySchedule = schedule.Availabilities.FirstOrDefault(a => a.DayOfWeek == query.Date.DayOfWeek);
        if (daySchedule == null)
        {
            return new AvailabilityDto(query.Date.DayOfWeek, []);
        }

        var dayBookings = await bookingRepository.GetActiveByProviderAndDateAsync(query.ProviderId, query.Date, cancellationToken);
        var occupiedSlots = dayBookings.Select(b => b.TimeSlot).ToList();

        var tz = TimeZoneResolver.ResolveTimeZone(schedule.TimeZoneId, logger);
        if (tz == null)
        {
            return Result<AvailabilityDto>.Failure(Error.BadRequest("Fuso horário do prestador inválido."));
        }

        var availableSlots = daySchedule.Slots
            .SelectMany(slot => slot.Subtract(occupiedSlots))
            .Select(s => 
            {
                var localStartDateTime = query.Date.ToDateTime(s.Start);
                var localEndDateTime = query.Date.ToDateTime(s.End);
                
                var utcStartDateTime = TimeZoneInfo.ConvertTimeToUtc(localStartDateTime, tz);
                var utcEndDateTime = TimeZoneInfo.ConvertTimeToUtc(localEndDateTime, tz);
                
                return new AvailableSlotDto(
                    new DateTimeOffset(utcStartDateTime, TimeSpan.Zero),
                    new DateTimeOffset(utcEndDateTime, TimeSpan.Zero));
            })
            .ToList();

        return new AvailabilityDto(query.Date.DayOfWeek, availableSlots);
    }
}
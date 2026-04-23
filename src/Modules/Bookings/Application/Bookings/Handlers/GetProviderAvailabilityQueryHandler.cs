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

        // Obtém apenas os agendamentos ativos para a data solicitada diretamente do repositório
        var dayBookings = await bookingRepository.GetActiveByProviderAndDateAsync(query.ProviderId, query.Date, cancellationToken);
        var occupiedSlots = dayBookings.Select(b => b.TimeSlot).ToList();

        // Resolve o fuso horário para retornar DateTimeOffset correto
        var tz = TimeZoneResolver.ResolveTimeZone(schedule.TimeZoneId, logger);
        if (tz == null)
        {
            return Result<AvailabilityDto>.Failure(Error.BadRequest("Fuso horário do prestador inválido."));
        }

        // Filtra os slots do schedule subtraindo os intervalos ocupados
        var availableSlots = daySchedule.Slots
            .SelectMany(slot => slot.Subtract(occupiedSlots))
            .Select(s => 
            {
                var startDateTime = query.Date.ToDateTime(s.Start);
                var endDateTime = query.Date.ToDateTime(s.End);
                
                // Converte para DateTimeOffset usando o fuso do prestador
                var startOffset = tz.GetUtcOffset(startDateTime);
                var endOffset = tz.GetUtcOffset(endDateTime);
                
                return new AvailableSlotDto(
                    new DateTimeOffset(startDateTime, startOffset),
                    new DateTimeOffset(endDateTime, endOffset));
            })
            .ToList();

        return new AvailabilityDto(query.Date.DayOfWeek, availableSlots);
    }
}

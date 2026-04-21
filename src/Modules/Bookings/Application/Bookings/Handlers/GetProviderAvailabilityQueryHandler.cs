using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
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

        var schedule = await scheduleRepository.GetByProviderIdAsync(query.ProviderId, cancellationToken);
        if (schedule == null)
        {
            return Result<AvailabilityDto>.Failure(Error.NotFound("Provider schedule not found."));
        }

        var daySchedule = schedule.Availabilities.FirstOrDefault(a => a.DayOfWeek == query.Date.DayOfWeek);
        if (daySchedule == null)
        {
            return new AvailabilityDto(query.Date.DayOfWeek, []);
        }

        var bookings = await bookingRepository.GetByProviderIdAsync(query.ProviderId, cancellationToken);
        var dayBookings = bookings
            .Where(b => b.TimeSlot.Start.Date == query.Date.Date && 
                        b.Status != Contracts.Bookings.Enums.EBookingStatus.Cancelled &&
                        b.Status != Contracts.Bookings.Enums.EBookingStatus.Rejected)
            .ToList();

        // Lógica simplificada: retorna os slots do schedule. 
        // Em uma implementação real, subtrairíamos os dayBookings dos slots disponíveis.
        // TODO: Refinar cálculo de slots livres subtraindo agendamentos existentes.

        var slots = daySchedule.Slots.Select(s => new TimeSlotDto(
            query.Date.Date.Add(s.Start.TimeOfDay), 
            query.Date.Date.Add(s.End.TimeOfDay)));

        return new AvailabilityDto(query.Date.DayOfWeek, slots);
    }
}

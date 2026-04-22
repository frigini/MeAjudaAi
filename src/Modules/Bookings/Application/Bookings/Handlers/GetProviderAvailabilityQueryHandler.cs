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
            return Result<AvailabilityDto>.Failure(Error.NotFound("Agenda do prestador não encontrada."));
        }

        var daySchedule = schedule.Availabilities.FirstOrDefault(a => a.DayOfWeek == query.Date.DayOfWeek);
        if (daySchedule == null)
        {
            return new AvailabilityDto(query.Date.DayOfWeek, []);
        }

        var bookings = await bookingRepository.GetByProviderIdAsync(query.ProviderId, cancellationToken);
        // Filtra bookings ativos para a data solicitada usando o novo campo Date
        var dayBookings = bookings
            .Where(b => b.Date == query.Date && 
                        b.Status != Contracts.Bookings.Enums.EBookingStatus.Cancelled &&
                        b.Status != Contracts.Bookings.Enums.EBookingStatus.Rejected &&
                        b.Status != Contracts.Bookings.Enums.EBookingStatus.Completed)
            .ToList();

        var occupiedSlots = dayBookings.Select(b => b.TimeSlot).ToList();

        // Filtra os slots do schedule subtraindo os intervalos ocupados
        var availableSlots = daySchedule.Slots
            .SelectMany(slot => slot.Subtract(occupiedSlots))
            .Select(s => new TimeSlotDto(s.Start, s.End))
            .ToList();

        return new AvailabilityDto(query.Date.DayOfWeek, availableSlots);
    }
}

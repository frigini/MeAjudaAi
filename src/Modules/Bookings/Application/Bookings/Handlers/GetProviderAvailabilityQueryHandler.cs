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
        var dayBookings = bookings
            .Where(b => DateOnly.FromDateTime(b.TimeSlot.Start) == query.Date && 
                        b.Status != Contracts.Bookings.Enums.EBookingStatus.Cancelled &&
                        b.Status != Contracts.Bookings.Enums.EBookingStatus.Rejected)
            .ToList();

        // Filtra os slots do schedule removendo aqueles que conflitam com bookings existentes
        var availableSlots = daySchedule.Slots
            .Select(s => new TimeSlotDto(
                query.Date.ToDateTime(TimeOnly.FromDateTime(s.Start)), 
                query.Date.ToDateTime(TimeOnly.FromDateTime(s.End))))
            .Where(slot => !dayBookings.Any(b => 
                slot.Start < b.TimeSlot.End && b.TimeSlot.Start < slot.End))
            .ToList();

        return new AvailabilityDto(query.Date.DayOfWeek, availableSlots);
    }
}

using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class CreateBookingCommandHandler(
    IBookingRepository bookingRepository,
    IProviderScheduleRepository scheduleRepository,
    IProvidersModuleApi providersApi,
    ILogger<CreateBookingCommandHandler> logger) : ICommandHandler<CreateBookingCommand, Result<BookingDto>>
{
    public async Task<Result<BookingDto>> HandleAsync(CreateBookingCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating booking for Provider {ProviderId} and Client {ClientId}", 
            command.ProviderId, command.ClientId);

        // 1. Validar existência do Provider
        var providerExists = await providersApi.ProviderExistsAsync(command.ProviderId, cancellationToken);
        if (providerExists.IsFailure || !providerExists.Value)
        {
            return Result<BookingDto>.Failure(Error.NotFound("Provider not found."));
        }

        // 2. Validar Horário de Trabalho (Schedule)
        var schedule = await scheduleRepository.GetByProviderIdAsync(command.ProviderId, cancellationToken);
        if (schedule == null)
        {
            return Result<BookingDto>.Failure(Error.BadRequest("Provider has no defined schedule."));
        }

        var duration = command.End - command.Start;
        if (!schedule.IsAvailable(command.Start, duration))
        {
            return Result<BookingDto>.Failure(Error.BadRequest("Provider is not available at the requested time."));
        }

        // 3. Validar Sobreposição (Overlaps)
        var hasOverlap = await bookingRepository.HasOverlapAsync(
            command.ProviderId, 
            command.Start, 
            command.End, 
            cancellationToken);

        if (hasOverlap)
        {
            return Result<BookingDto>.Failure(Error.Conflict("There is already a booking for this provider in the requested time."));
        }

        // 4. Criar Booking
        var timeSlot = TimeSlot.Create(command.Start, command.End);
        var booking = Booking.Create(
            command.ProviderId, 
            command.ClientId, 
            command.ServiceId, 
            timeSlot);

        await bookingRepository.AddAsync(booking, cancellationToken);

        logger.LogInformation("Booking {BookingId} created successfully.", booking.Id);

        return new BookingDto(
            booking.Id,
            booking.ProviderId,
            booking.ClientId,
            booking.ServiceId,
            booking.TimeSlot.Start,
            booking.TimeSlot.End,
            booking.Status);
    }
}

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

        // 0. Validar Intervalo
        if (command.End <= command.Start)
        {
            return Result<BookingDto>.Failure(Error.BadRequest("O horário de término deve ser após o horário de início."));
        }

        // 1. Validar existência do Provider
        var providerExists = await providersApi.ProviderExistsAsync(command.ProviderId, cancellationToken);
        if (providerExists.IsFailure || !providerExists.Value)
        {
            return Result<BookingDto>.Failure(Error.NotFound("Prestador não encontrado."));
        }

        // 2. Validar Horário de Trabalho (Schedule)
        var schedule = await scheduleRepository.GetByProviderIdAsync(command.ProviderId, cancellationToken);
        if (schedule == null)
        {
            return Result<BookingDto>.Failure(Error.BadRequest("Prestador não possui agenda configurada."));
        }

        var duration = command.End - command.Start;
        if (!schedule.IsAvailable(command.Start.UtcDateTime, duration))
        {
            return Result<BookingDto>.Failure(Error.BadRequest("Prestador indisponível no horário solicitado."));
        }

        // 3. Criar e Tentar Adicionar atomicamente
        var timeSlot = TimeSlot.Create(command.Start.UtcDateTime, command.End.UtcDateTime);
        var booking = Booking.Create(
            command.ProviderId, 
            command.ClientId, 
            command.ServiceId, 
            timeSlot);

        var result = await bookingRepository.AddIfNoOverlapAsync(booking, cancellationToken);
        
        if (result.IsFailure)
        {
            return Result<BookingDto>.Failure(result.Error!);
        }

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

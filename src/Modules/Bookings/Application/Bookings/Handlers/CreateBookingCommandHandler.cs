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

        if (command.Start <= DateTimeOffset.UtcNow)
        {
            return Result<BookingDto>.Failure(Error.BadRequest("O horário de início deve ser no futuro."));
        }

        // 1. Validar existência do Provider
        var providerExists = await providersApi.ProviderExistsAsync(command.ProviderId, cancellationToken);
        if (providerExists.IsFailure)
        {
            return Result<BookingDto>.Failure(providerExists.Error);
        }
        
        if (!providerExists.Value)
        {
            return Result<BookingDto>.Failure(Error.NotFound("Prestador não encontrado."));
        }

        // 2. Validar Horário de Trabalho (Schedule)
        var schedule = await scheduleRepository.GetByProviderIdAsync(command.ProviderId, cancellationToken);
        if (schedule == null)
        {
            return Result<BookingDto>.Failure(Error.BadRequest("Prestador não possui agenda configurada."));
        }

        // Converte o início para o fuso horário local do prestador para validar DayOfWeek corretamente
        DateTime localStartTime;
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(schedule.TimeZoneId);
            localStartTime = TimeZoneInfo.ConvertTimeFromUtc(command.Start.UtcDateTime, tz);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            logger.LogWarning(ex, "TimeZoneId {TimeZoneId} not found. Falling back to UTC.", schedule.TimeZoneId);
            localStartTime = command.Start.UtcDateTime;
        }

        var duration = command.End - command.Start;
        if (!schedule.IsAvailable(localStartTime, duration))
        {
            return Result<BookingDto>.Failure(Error.BadRequest("Prestador indisponível no horário solicitado."));
        }

        // 3. Criar e Tentar Adicionar atomicamente
        var date = DateOnly.FromDateTime(command.Start.UtcDateTime);
        var timeSlot = TimeSlot.FromDateTime(command.Start.UtcDateTime, command.End.UtcDateTime);
        var booking = Booking.Create(
            command.ProviderId, 
            command.ClientId, 
            command.ServiceId, 
            date,
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
            booking.Date.ToDateTime(booking.TimeSlot.Start),
            booking.Date.ToDateTime(booking.TimeSlot.End),
            booking.Status);
    }
}

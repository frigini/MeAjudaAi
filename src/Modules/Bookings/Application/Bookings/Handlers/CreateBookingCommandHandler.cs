using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Common;
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
    IServiceCatalogsModuleApi serviceCatalogsApi,
    ILogger<CreateBookingCommandHandler> logger) : ICommandHandler<CreateBookingCommand, Result<BookingDto>>
{
    public async Task<Result<BookingDto>> HandleAsync(CreateBookingCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating booking for Provider {ProviderId}", command.ProviderId);
        logger.LogDebug("Creating booking for Client {ClientId}", command.ClientId);

        // 0. Validar Intervalo
        if (command.End <= command.Start)
        {
            return Result<BookingDto>.Failure(Error.BadRequest("O horário de término deve ser após o horário de início.", ErrorCodes.Bookings.InvalidTime));
        }

        // Tolerância de 1 minuto para agendamentos imediatos
        var minimumLead = TimeSpan.FromMinutes(1);
        if (command.Start < DateTimeOffset.UtcNow.Add(minimumLead))
        {
            return Result<BookingDto>.Failure(Error.BadRequest("O horário de início deve ser no futuro (mínimo 1 minuto de antecedência).", ErrorCodes.Bookings.StartNotInFuture));
        }

        // 1. Validar existência do Provider, Atividade do Serviço e Agenda em paralelo
        var providerExistsTask = providersApi.ProviderExistsAsync(command.ProviderId, cancellationToken);
        var serviceActiveTask = serviceCatalogsApi.IsServiceActiveAsync(command.ServiceId, cancellationToken);
        var scheduleTask = scheduleRepository.GetByProviderIdReadOnlyAsync(command.ProviderId, cancellationToken);

        await Task.WhenAll(providerExistsTask, serviceActiveTask, scheduleTask);

        var providerExists = await providerExistsTask;
        if (providerExists.IsFailure)
        {
            return Result<BookingDto>.Failure(providerExists.Error);
        }
        
        if (!providerExists.Value)
        {
            return Result<BookingDto>.Failure(Error.NotFound("Prestador não encontrado.", ErrorCodes.Providers.ProviderNotFound));
        }

        var serviceActive = await serviceActiveTask;
        if (serviceActive.IsFailure)
        {
            return Result<BookingDto>.Failure(serviceActive.Error);
        }
        
        if (!serviceActive.Value)
        {
            return Result<BookingDto>.Failure(Error.NotFound("Serviço não encontrado ou inativo.", ErrorCodes.Catalogs.ServiceNotFound));
        }

        var schedule = await scheduleTask;
        if (schedule == null)
        {
            return Result<BookingDto>.Failure(Error.BadRequest("Prestador não possui agenda configurada.", ErrorCodes.Providers.ScheduleNotFound));
        }

        // 1.7 Validar posse do serviço pelo prestador (depende da validade anterior)
        var serviceOffered = await providersApi.IsServiceOfferedByProviderAsync(command.ProviderId, command.ServiceId, cancellationToken);
        if (serviceOffered.IsFailure)
        {
            return Result<BookingDto>.Failure(serviceOffered.Error);
        }

        if (!serviceOffered.Value)
        {
            return Result<BookingDto>.Failure(Error.NotFound("Serviço não encontrado ou não oferecido por este prestador.", ErrorCodes.Providers.ServiceNotOffered));
        }

        // Converte o início para o fuso horário local do prestador para validar DayOfWeek corretamente
        var tz = TimeZoneResolver.ResolveTimeZone(schedule.TimeZoneId, logger, allowFallback: false);
        if (tz == null)
        {
            return Result<BookingDto>.Failure(Error.BadRequest("Fuso horário do prestador inválido.", ErrorCodes.Bookings.InvalidTime));
        }

        var localStartTime = TimeZoneInfo.ConvertTimeFromUtc(command.Start.UtcDateTime, tz);

        var duration = command.End - command.Start;
        // Nota: duration é baseado em UTC e pode variar o horário local em transições de DST
        if (!schedule.IsAvailable(localStartTime, duration))
        {
            return Result<BookingDto>.Failure(Error.BadRequest("Prestador indisponível no horário solicitado.", ErrorCodes.Providers.ProviderUnavailable));
        }

        // 3. Criar booking para validação
        var localEndTime = localStartTime.Add(duration);

        // 3.1 Validar se o agendamento cruza a meia-noite (não suportado pelo modelo TimeSlot atual)
        if (localStartTime.Date != localEndTime.Date)
        {
            return Result<BookingDto>.Failure(Error.BadRequest("Agendamentos não podem cruzar a meia-noite. Por favor, divida em dois agendamentos distintos.", ErrorCodes.Bookings.MidnightSpanning));
        }

        var date = DateOnly.FromDateTime(localStartTime);
        var timeSlot = TimeSlot.FromDateTime(localStartTime, localEndTime);
        
        var booking = Booking.Create(
            command.ProviderId, 
            command.ClientId, 
            command.ServiceId, 
            date,
            timeSlot);

        // 4. Validar DTO antes de persistir
        var dtoResult = TimeZoneResolver.CreateValidatedBookingDto(booking, tz, logger);
        if (dtoResult.IsFailure)
        {
            return Result<BookingDto>.Failure(dtoResult.Error);
        }

        // 5. Persistir atomicamente
        var result = await bookingRepository.AddIfNoOverlapAsync(booking, cancellationToken);
        
        if (result.IsFailure)
        {
            // Preserva o código de erro original (ex: Overlap ou ConcurrencyConflict)
            return Result<BookingDto>.Failure(result.Error!);
        }

        logger.LogInformation("Booking {BookingId} created successfully.", booking.Id);

        return dtoResult;
    }
}

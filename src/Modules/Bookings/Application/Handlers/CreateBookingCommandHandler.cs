using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Application.Services;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Handlers;

public sealed class CreateBookingCommandHandler(
    IBookingCommandService bookingCommandService,
    IProviderScheduleQueries scheduleQueries,
    IProvidersModuleApi providersApi,
    IServiceCatalogsModuleApi serviceCatalogsApi,
    ILogger<CreateBookingCommandHandler> logger) : ICommandHandler<CreateBookingCommand, Result<ModuleBookingDto>>
{
    public async Task<Result<ModuleBookingDto>> HandleAsync(CreateBookingCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating booking for Provider {ProviderId}", command.ProviderId);
        logger.LogDebug("Creating booking for Client {ClientId}", command.ClientId);

        if (command.End <= command.Start)
        {
            return Result<ModuleBookingDto>.Failure(Error.BadRequest("O horário de término deve ser após o horário de início.", ErrorCodes.Bookings.InvalidTime));
        }

        var minimumLead = TimeSpan.FromMinutes(1);
        if (command.Start < DateTimeOffset.UtcNow.Add(minimumLead))
        {
            return Result<ModuleBookingDto>.Failure(Error.BadRequest("O horário de início deve ser no futuro (mínimo 1 minuto de antecedência).", ErrorCodes.Bookings.StartNotInFuture));
        }

        var providerExistsTask = providersApi.ProviderExistsAsync(command.ProviderId, cancellationToken);
        var serviceActiveTask = serviceCatalogsApi.IsServiceActiveAsync(command.ServiceId, cancellationToken);
        var scheduleTask = scheduleQueries.GetByProviderIdReadOnlyAsync(command.ProviderId, cancellationToken);

        await Task.WhenAll(providerExistsTask, serviceActiveTask, scheduleTask);

        var providerExists = await providerExistsTask;
        if (providerExists.IsFailure)
        {
            return Result<ModuleBookingDto>.Failure(providerExists.Error);
        }

        if (!providerExists.Value)
        {
            return Result<ModuleBookingDto>.Failure(Error.NotFound("Prestador não encontrado.", ErrorCodes.Providers.ProviderNotFound));
        }

        var serviceActive = await serviceActiveTask;
        if (serviceActive.IsFailure)
        {
            return Result<ModuleBookingDto>.Failure(serviceActive.Error);
        }

        if (!serviceActive.Value)
        {
            return Result<ModuleBookingDto>.Failure(Error.NotFound("Serviço não encontrado ou inativo.", ErrorCodes.Catalogs.ServiceNotFound));
        }

        var schedule = await scheduleTask;
        if (schedule == null)
        {
            return Result<ModuleBookingDto>.Failure(Error.BadRequest("Prestador não possui agenda configurada.", ErrorCodes.Providers.ScheduleNotFound));
        }

        var serviceOffered = await providersApi.IsServiceOfferedByProviderAsync(command.ProviderId, command.ServiceId, cancellationToken);
        if (serviceOffered.IsFailure)
        {
            return Result<ModuleBookingDto>.Failure(serviceOffered.Error);
        }

        if (!serviceOffered.Value)
        {
            return Result<ModuleBookingDto>.Failure(Error.NotFound("Serviço não encontrado ou não oferecido por este prestador.", ErrorCodes.Providers.ServiceNotOffered));
        }

        var tz = TimeZoneResolver.ResolveTimeZone(schedule.TimeZoneId, logger, allowFallback: false);
        if (tz == null)
        {
            return Result<ModuleBookingDto>.Failure(Error.BadRequest("Fuso horário do prestador inválido.", ErrorCodes.Bookings.InvalidTime));
        }

        var localStartTime = TimeZoneInfo.ConvertTimeFromUtc(command.Start.UtcDateTime, tz);

        var duration = command.End - command.Start;
        if (!schedule.IsAvailable(localStartTime, duration))
        {
            return Result<ModuleBookingDto>.Failure(Error.BadRequest("Prestador indisponível no horário solicitado.", ErrorCodes.Providers.ProviderUnavailable));
        }

        var localEndTime = localStartTime.Add(duration);

        if (localStartTime.Date != localEndTime.Date)
        {
            return Result<ModuleBookingDto>.Failure(Error.BadRequest("Agendamentos não podem cruzar a meia-noite. Por favor, divida em dois agendamentos distintos.", ErrorCodes.Bookings.MidnightSpanning));
        }

        var date = DateOnly.FromDateTime(localStartTime);
        var timeSlot = TimeSlot.FromDateTime(localStartTime, localEndTime);

        var booking = Booking.Create(
            command.ProviderId,
            command.ClientId,
            command.ServiceId,
            date,
            timeSlot);

        var dtoResult = TimeZoneResolver.CreateValidatedBookingDto(booking, tz, logger);
        if (dtoResult.IsFailure)
        {
            return Result<ModuleBookingDto>.Failure(dtoResult.Error);
        }

        var result = await bookingCommandService.AddIfNoOverlapAsync(booking, cancellationToken);

        if (result.IsFailure)
        {
            return Result<ModuleBookingDto>.Failure(result.Error!);
        }

        logger.LogInformation("Booking {BookingId} created successfully.", booking.Id);

        return dtoResult;
    }
}

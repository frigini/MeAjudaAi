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
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Handlers.Commands;

/// <summary>
/// Handler para processar comandos de criação de booking.
/// </summary>
public sealed class CreateBookingCommandHandler(
    IBookingCommandService bookingCommandService,
    IProviderScheduleQueries scheduleQueries,
    IProvidersModuleApi providersApi,
    IServiceCatalogsModuleApi serviceCatalogsApi,
    ILogger<CreateBookingCommandHandler> logger,
    IStringLocalizer<Strings> localizer) : ICommandHandler<CreateBookingCommand, Result<ModuleBookingDto>>
{
    public async Task<Result<ModuleBookingDto>> HandleAsync(CreateBookingCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating booking for Provider {ProviderId}", command.ProviderId);
        logger.LogDebug("Creating booking for Client {ClientId}", command.ClientId);

        var minimumLead = TimeSpan.FromMinutes(1);
        if (command.Start < DateTimeOffset.UtcNow.Add(minimumLead))
        {
            return Result<ModuleBookingDto>.Failure(Error.BadRequest(localizer["BookingStartTimeMustBeFuture"], ErrorCodes.Bookings.StartNotInFuture));
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
            return Result<ModuleBookingDto>.Failure(Error.NotFound(localizer["ProviderNotFound"], ErrorCodes.Providers.ProviderNotFound));
        }

        var serviceActive = await serviceActiveTask;
        if (serviceActive.IsFailure)
        {
            return Result<ModuleBookingDto>.Failure(serviceActive.Error);
        }

        if (!serviceActive.Value)
        {
            return Result<ModuleBookingDto>.Failure(Error.NotFound(localizer["ServiceNotActive"], ErrorCodes.Catalogs.ServiceNotFound));
        }

        var schedule = await scheduleTask;
        if (schedule == null)
        {
            return Result<ModuleBookingDto>.Failure(Error.BadRequest(localizer["ProviderScheduleNotFound"], ErrorCodes.Providers.ScheduleNotFound));
        }

        var serviceOffered = await providersApi.IsServiceOfferedByProviderAsync(command.ProviderId, command.ServiceId, cancellationToken);
        if (serviceOffered.IsFailure)
        {
            return Result<ModuleBookingDto>.Failure(serviceOffered.Error);
        }

        if (!serviceOffered.Value)
        {
            return Result<ModuleBookingDto>.Failure(Error.NotFound(localizer["ServiceNotOfferedByProvider"], ErrorCodes.Providers.ServiceNotOffered));
        }

        var tz = TimeZoneResolver.ResolveTimeZone(schedule.TimeZoneId, logger, allowFallback: false);
        if (tz == null)
        {
            return Result<ModuleBookingDto>.Failure(Error.BadRequest(localizer["InvalidTimezone"], ErrorCodes.Bookings.InvalidTime));
        }

        var localStartTime = TimeZoneInfo.ConvertTimeFromUtc(command.Start.UtcDateTime, tz);

        var duration = command.End - command.Start;
        if (!schedule.IsAvailable(localStartTime, duration))
        {
            return Result<ModuleBookingDto>.Failure(Error.BadRequest(localizer["ProviderUnavailable"], ErrorCodes.Providers.ProviderUnavailable));
        }

        var localEndTime = localStartTime.Add(duration);

        if (localStartTime.Date != localEndTime.Date)
        {
            return Result<ModuleBookingDto>.Failure(Error.BadRequest(localizer["BookingCrossesMidnight"], ErrorCodes.Bookings.MidnightSpanning));
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

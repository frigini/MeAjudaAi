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

public sealed class SetProviderScheduleCommandHandler(
    IProviderScheduleRepository scheduleRepository,
    IProvidersModuleApi providersApi,
    ILogger<SetProviderScheduleCommandHandler> logger) : ICommandHandler<SetProviderScheduleCommand, Result>
{
    public async Task<Result> HandleAsync(SetProviderScheduleCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Setting schedule for Provider {ProviderId}", command.ProviderId);

        // 1. Validar existência do Provider
        var providerExists = await providersApi.ProviderExistsAsync(command.ProviderId, cancellationToken);
        if (providerExists.IsFailure || !providerExists.Value)
        {
            return Result.Failure(Error.NotFound("Provider not found."));
        }

        // 2. Buscar ou criar Schedule
        var schedule = await scheduleRepository.GetByProviderIdAsync(command.ProviderId, cancellationToken);
        bool isNew = false;

        if (schedule == null)
        {
            schedule = ProviderSchedule.Create(command.ProviderId);
            isNew = true;
        }

        // 3. Atualizar Disponibilidades
        try
        {
            foreach (var availabilityDto in command.Availabilities)
            {
                var slots = availabilityDto.Slots.Select(s => TimeSlot.Create(s.Start, s.End));
                var availability = Availability.Create(availabilityDto.DayOfWeek, slots);
                schedule.SetAvailability(availability);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Invalid availability data provided for Provider {ProviderId}", command.ProviderId);
            return Result.Failure(Error.BadRequest(ex.Message));
        }

        // 4. Persistir
        if (isNew)
        {
            await scheduleRepository.AddAsync(schedule, cancellationToken);
        }
        else
        {
            await scheduleRepository.UpdateAsync(schedule, cancellationToken);
        }

        logger.LogInformation("Schedule for Provider {ProviderId} updated successfully.", command.ProviderId);

        return Result.Success();
    }
}

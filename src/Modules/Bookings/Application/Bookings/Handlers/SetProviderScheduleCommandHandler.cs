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

        if (command.Availabilities == null)
        {
            logger.LogWarning("Availabilities is null for Provider {ProviderId}", command.ProviderId);
            return Result.Failure(Error.BadRequest("A lista de disponibilidades não pode ser nula."));
        }

        // 1. Validar existência do Provider
        var providerExists = await providersApi.ProviderExistsAsync(command.ProviderId, cancellationToken);
        if (providerExists.IsFailure)
        {
            return Result.Failure(providerExists.Error);
        }
        
        if (!providerExists.Value)
        {
            return Result.Failure(Error.NotFound("Prestador não encontrado."));
        }

        // 2. Pré-validar e montar objetos de Domínio (Fail-fast antes de alterar estado do aggregate)
        var newAvailabilities = new List<Availability>();
        try
        {
            foreach (var availabilityDto in command.Availabilities)
            {
                var slots = availabilityDto.Slots.Select(s => TimeSlot.Create(s.Start, s.End));
                var availability = Availability.Create(availabilityDto.DayOfWeek, slots);
                newAvailabilities.Add(availability);
            }
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            logger.LogWarning(ex, "Invalid availability data provided for Provider {ProviderId}", command.ProviderId);
            return Result.Failure(Error.BadRequest("Os dados de horário fornecidos são inválidos. Verifique sobreposições ou horários negativos."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error processing availabilities for Provider {ProviderId}", command.ProviderId);
            return Result.Failure(Error.Internal("Erro interno ao processar disponibilidades."));
        }

        // 3. Buscar ou criar Schedule
        var schedule = await scheduleRepository.GetByProviderIdAsync(command.ProviderId, cancellationToken);
        bool isNew = false;

        if (schedule == null)
        {
            schedule = ProviderSchedule.Create(command.ProviderId);
            isNew = true;
        }
        else
        {
            // Limpa as disponibilidades existentes
            schedule.ClearAvailabilities();
        }

        // Aplica validações já efetuadas
        foreach (var availability in newAvailabilities)
        {
            schedule.SetAvailability(availability);
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

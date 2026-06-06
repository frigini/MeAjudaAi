using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Application.Bookings.Handlers;

public sealed class SetProviderScheduleCommandHandler(
    IProviderScheduleQueries scheduleQueries,
    [FromKeyedServices(ModuleKeys.Bookings)] IUnitOfWork uow,
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

        var providerExists = await providersApi.ProviderExistsAsync(command.ProviderId, cancellationToken);
        if (providerExists.IsFailure)
        {
            return Result.Failure(providerExists.Error);
        }
        
        if (!providerExists.Value)
        {
            return Result.Failure(Error.NotFound("Prestador não encontrado."));
        }

        var newAvailabilities = new List<Availability>();
        try
        {
            foreach (var availabilityDto in command.Availabilities)
            {
                if (availabilityDto == null)
                {
                    return Result.Failure(Error.BadRequest("Uma das disponibilidades fornecidas é nula."));
                }

                if (availabilityDto.Slots == null)
                {
                    return Result.Failure(Error.BadRequest($"A lista de horários para {availabilityDto.DayOfWeek} não pode ser nula."));
                }

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

        var schedule = await scheduleQueries.GetByProviderIdAsync(command.ProviderId, cancellationToken);
        
        if (schedule == null)
        {
            schedule = ProviderSchedule.Create(command.ProviderId);
            
            foreach (var availability in newAvailabilities)
            {
                schedule.SetAvailability(availability);
            }

            try 
            {
                uow.GetRepository<ProviderSchedule, Guid>().Add(schedule);
                await uow.SaveChangesAsync(cancellationToken);
                logger.LogInformation("New schedule for Provider {ProviderId} created successfully.", command.ProviderId);
                return Result.Success();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                logger.LogWarning("Conflict detected while adding schedule for Provider {ProviderId}. Falling back to update.", command.ProviderId);
                schedule = await scheduleQueries.GetByProviderIdAsync(command.ProviderId, cancellationToken);
                
                if (schedule == null) throw;
            }
        }

        schedule.ClearAvailabilities();
        foreach (var availability in newAvailabilities)
        {
            schedule.SetAvailability(availability);
        }

        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Schedule for Provider {ProviderId} updated successfully.", command.ProviderId);

        return Result.Success();
    }
}




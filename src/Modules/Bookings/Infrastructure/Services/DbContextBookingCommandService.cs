using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Bookings.Application.Services;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Contracts.Bookings.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Services;

public class DbContextBookingCommandService(
    BookingsDbContext context,
    ILogger<DbContextBookingCommandService> logger) : IBookingCommandService
{
    public async Task<Result> AddIfNoOverlapAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            const int maxRetryAttempts = 3;
            var attempt = 0;

            while (true)
            {
                attempt++;
                await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

                try
                {
                    var existingBookings = await context.Bookings
                        .Where(b => b.Id == booking.Id ||
                                   (b.ProviderId == booking.ProviderId &&
                                    b.Date == booking.Date &&
                                    b.Status != EBookingStatus.Cancelled &&
                                    b.Status != EBookingStatus.Rejected &&
                                    b.Status != EBookingStatus.Completed &&
                                    b.TimeSlot.Start < booking.TimeSlot.End &&
                                    booking.TimeSlot.Start < b.TimeSlot.End))
                        .Select(b => new { b.Id })
                        .ToListAsync(cancellationToken);

                    if (existingBookings.Any(b => b.Id == booking.Id))
                    {
                        logger.LogInformation("Booking {BookingId} already exists. Returning success (Idempotent).", booking.Id);
                        return Result.Success();
                    }

                    if (existingBookings.Any())
                    {
                        return Result.Failure(Error.Conflict("Já existe um agendamento para este horário.", ErrorCodes.Bookings.Overlap));
                    }

                    context.Bookings.Add(booking);
                    await context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return Result.Success();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    if (IsConcurrencyError(ex) && attempt < maxRetryAttempts)
                    {
                        logger.LogWarning("Concurrency conflict while validating booking {BookingId}. Retrying (Attempt {Attempt})...", booking.Id, attempt);

                        try
                        {
                            await transaction.RollbackAsync(CancellationToken.None);
                        }
                        catch (Exception rollbackEx)
                        {
                            logger.LogDebug("Rollback failed during retry (expected in some scenarios): {Error}", rollbackEx.Message);
                        }

                        var baseJitter = Random.Shared.Next(50, 200);
                        var delay = Math.Min((int)(baseJitter * Math.Pow(2, attempt - 1)), 2000);
                        await Task.Delay(delay, cancellationToken);

                        context.Entry(booking).State = EntityState.Detached;
                        continue;
                    }

                    context.Entry(booking).State = EntityState.Detached;

                    if (IsConcurrencyError(ex))
                    {
                        logger.LogWarning(ex, "Concurrency conflict after max retries while attempting to add booking {BookingId} (Attempt {Attempt})", booking.Id, attempt);
                        return Result.Failure(Error.Conflict("Conflito de concorrência ao validar agendamento. Tente novamente em instantes.", ErrorCodes.Bookings.ConcurrencyConflict));
                    }

                    logger.LogError(ex, "Fatal error while attempting to add booking {BookingId} (Attempt {Attempt})", booking.Id, attempt);
                    throw;
                }
            }
        });
    }

    private static bool IsConcurrencyError(Exception ex)
    {
        var currentEx = ex;
        while (currentEx != null)
        {
            if (currentEx is PostgresException { SqlState: "40001" or "40P01" })
            {
                return true;
            }
            currentEx = currentEx.InnerException;
        }
        return false;
    }
}

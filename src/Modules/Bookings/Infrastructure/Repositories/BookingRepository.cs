using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Contracts.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Contracts.Utilities.Constants;
using System.Data;
using Npgsql;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Repositories;

public class BookingRepository(BookingsDbContext context, ILogger<BookingRepository> logger) : IBookingRepository
{
    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Booking?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetByProviderIdPagedAsync(Guid providerId, DateOnly? from, DateOnly? to, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Bookings
            .AsNoTracking()
            .Where(b => b.ProviderId == providerId);

        return await GetBookingsPagedAsync(query, from, to, page, pageSize, cancellationToken);
    }

    public async Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetByClientIdPagedAsync(Guid clientId, DateOnly? from, DateOnly? to, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Bookings
            .AsNoTracking()
            .Where(b => b.ClientId == clientId);

        return await GetBookingsPagedAsync(query, from, to, page, pageSize, cancellationToken);
    }

    private async Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetBookingsPagedAsync(
        IQueryable<Booking> query, 
        DateOnly? from, 
        DateOnly? to, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        if (from.HasValue) query = query.Where(b => b.Date >= from.Value);
        if (to.HasValue) query = query.Where(b => b.Date <= to.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(b => b.Date)
            .ThenByDescending(b => b.TimeSlot.Start)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Booking>> GetActiveByProviderAndDateAsync(Guid providerId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .AsNoTracking()
            .Where(b => b.ProviderId == providerId && 
                        b.Date == date && 
                        b.Status != EBookingStatus.Cancelled &&
                        b.Status != EBookingStatus.Rejected &&
                        b.Status != EBookingStatus.Completed)
            .ToListAsync(cancellationToken);
    }

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
                    // Combina verificação de idempotência (Id igual) e sobreposição no mesmo Provider/Data
                    // Filtramos por Id igual OU (Provider igual E Data igual E Status ativo E Horários cruzados)
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
                    // Re-lançamento intencional: a transação é gerida via await using e DisposeAsync fará rollback automaticamente.
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

                        // Exponential backoff with jitter
                        var baseJitter = Random.Shared.Next(50, 200);
                        var delay = Math.Min((int)(baseJitter * Math.Pow(2, attempt - 1)), 2000);
                        await Task.Delay(delay, cancellationToken);
                        
                        context.Entry(booking).State = EntityState.Detached;
                        continue;
                    }

                    // Garante que a entidade seja desanexada em caso de falha fatal ou esgotamento de retries
                    // para evitar que o ChangeTracker tente inseri-la novamente se o DbContext for reutilizado.
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

    public async Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        try
        {
            context.Bookings.Update(booking);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException("O agendamento foi modificado por outro usuário. Por favor, recarregue os dados.", ex);
        }
    }
}
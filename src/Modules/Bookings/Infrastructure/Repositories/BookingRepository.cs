using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Contracts.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Exceptions;
using System.Data;
using Npgsql;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Repositories;

public class BookingRepository(BookingsDbContext context, ILogger<BookingRepository> logger) : IBookingRepository
{
    /// <summary>
    /// Obtém um agendamento pelo ID.
    /// Deliberadamente não utiliza AsNoTracking pois o objeto retornado é comumente 
    /// utilizado para atualizações subsequentes via UpdateAsync (ex: Confirm/Cancel).
    /// </summary>
    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    [Obsolete]
    public async Task<IReadOnlyList<Booking>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .AsNoTracking()
            .Where(b => b.ProviderId == providerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetByProviderIdPagedAsync(Guid providerId, DateOnly? from, DateOnly? to, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Bookings
            .AsNoTracking()
            .Where(b => b.ProviderId == providerId);

        return await GetBookingsPagedAsync(query, from, to, page, pageSize, cancellationToken);
    }

    [Obsolete]
    public async Task<IReadOnlyList<Booking>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .AsNoTracking()
            .Where(b => b.ClientId == clientId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
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
        // Normalizar paginação
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

    [Obsolete]
    public async Task<IReadOnlyList<Booking>> GetByProviderAndStatusAsync(Guid providerId, EBookingStatus status, CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .AsNoTracking()
            .Where(b => b.ProviderId == providerId && b.Status == status)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
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

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        await context.Bookings.AddAsync(booking, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
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
                context.ChangeTracker.Clear();
                await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                
                try
                {
                    // Verificação idempotente: se o agendamento com este ID já existir, 
                    // significa que uma tentativa anterior teve sucesso mas o cliente não recebeu a resposta.
                    var alreadyExists = await context.Bookings.AnyAsync(b => b.Id == booking.Id, cancellationToken);
                    if (alreadyExists)
                    {
                        logger.LogInformation("Booking {BookingId} already exists. Returning success (Idempotent).", booking.Id);
                        return Result.Success();
                    }

                    // NOTA: Agora incluímos a data no predicado para evitar conflitos em dias diferentes
                    var hasOverlap = await context.Bookings
                        .AnyAsync(b => 
                            b.ProviderId == booking.ProviderId &&
                            b.Date == booking.Date &&
                            b.Status != EBookingStatus.Cancelled &&
                            b.Status != EBookingStatus.Rejected &&
                            b.Status != EBookingStatus.Completed &&
                            b.TimeSlot.Start < booking.TimeSlot.End &&
                            booking.TimeSlot.Start < b.TimeSlot.End,
                            cancellationToken);

                    if (hasOverlap)
                    {
                        return Result.Failure(Error.Conflict("Já existe um agendamento para este prestador no período solicitado."));
                    }

                    await context.Bookings.AddAsync(booking, cancellationToken);
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
                    // Checa por conflitos de concorrência (40001 ou 40P01) PRIMEIRO
                    if (IsConcurrencyError(ex) && attempt < maxRetryAttempts)
                    {
                        logger.LogWarning("Concurrency conflict while validating booking {BookingId}. Retrying (Attempt {Attempt})...", booking.Id, attempt);

                        try
                        {
                            await transaction.RollbackAsync(CancellationToken.None);
                        }
                        catch
                        {
                            // Ignora erro de rollback
                        }

                        // Aguarda um tempo aleatório curto antes de tentar novamente (jitter)
                        await Task.Delay(Random.Shared.Next(50, 200), cancellationToken);
                        continue;
                    }

                    logger.LogError(ex, "Error while attempting to add booking {BookingId} (Attempt {Attempt})", booking.Id, attempt);

                    try
                    {
                        await transaction.RollbackAsync(CancellationToken.None);
                    }
                    catch
                    {
                        // Ignora erro de rollback
                    }
                    
                    if (IsConcurrencyError(ex))
                    {
                        return Result.Failure(Error.Conflict("Conflito de concorrência ao validar agendamento. Tente novamente em instantes."));
                    }

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

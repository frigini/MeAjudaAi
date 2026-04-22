using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Contracts.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Exceptions;
using System.Data;
using Npgsql;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Repositories;

public class BookingRepository(BookingsDbContext context) : IBookingRepository
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

    public async Task<IReadOnlyList<Booking>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .AsNoTracking()
            .Where(b => b.ProviderId == providerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .AsNoTracking()
            .Where(b => b.ClientId == clientId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetByProviderAndStatusAsync(Guid providerId, EBookingStatus status, CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .AsNoTracking()
            .Where(b => b.ProviderId == providerId && b.Status == status)
            .OrderByDescending(b => b.CreatedAt)
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
            // Usa transação Serializable para garantir atomicidade total do check-and-insert
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            
            try
            {
                // NOTA: Agora incluímos a data no predicado para evitar conflitos em dias diferentes
                var hasOverlap = await context.Bookings
                    .AnyAsync(b => 
                        b.ProviderId == booking.ProviderId &&
                        b.Date == booking.Date &&
                        b.Status != EBookingStatus.Cancelled &&
                        b.Status != EBookingStatus.Rejected &&
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
                // Rethrow cancellation immediately without rollback attempt
                throw;
            }
            catch (Exception ex)
            {
                try
                {
                    // Usa CancellationToken.None para garantir que o rollback ocorra mesmo se o token principal foi cancelado
                    await transaction.RollbackAsync(CancellationToken.None);
                }
                catch
                {
                    // Ignora erro de rollback se a transação já tiver sido abortada pelo banco
                }
                
                // Tratamento robusto para erros de serialização do PostgreSQL (40001) ou Deadlocks (40P01)
                // Checa recursivamente por PostgresException devido ao wrapping do EF Core e NpgsqlExecutionStrategy
                var currentEx = ex;
                while (currentEx != null)
                {
                    if (currentEx is PostgresException { SqlState: "40001" or "40P01" })
                    {
                        return Result.Failure(Error.Conflict("Conflito de concorrência ao validar agendamento. Tente novamente em instantes."));
                    }
                    currentEx = currentEx.InnerException;
                }

                throw;
            }
        });
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

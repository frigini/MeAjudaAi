using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Contracts.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Repositories;

public class BookingRepository(BookingsDbContext context) : IBookingRepository
{
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
            .OrderByDescending(b => b.TimeSlot.Start)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .AsNoTracking()
            .Where(b => b.ClientId == clientId)
            .OrderByDescending(b => b.TimeSlot.Start)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetByProviderAndStatusAsync(Guid providerId, EBookingStatus status, CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .AsNoTracking()
            .Where(b => b.ProviderId == providerId && b.Status == status)
            .OrderByDescending(b => b.TimeSlot.Start)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        await context.Bookings.AddAsync(booking, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        context.Bookings.Update(booking);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasOverlapAsync(Guid providerId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        // Um agendamento sobrepõe se:
        // (BookingStart < RequestEnd) AND (RequestStart < BookingEnd)
        // E apenas para agendamentos não cancelados/rejeitados
        return await context.Bookings
            .AnyAsync(b => 
                b.ProviderId == providerId &&
                b.Status != EBookingStatus.Cancelled &&
                b.Status != EBookingStatus.Rejected &&
                b.TimeSlot.Start < end &&
                start < b.TimeSlot.End,
                cancellationToken);
    }
}

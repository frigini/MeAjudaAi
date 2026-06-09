using MeAjudaAi.Contracts.Modules.Bookings.Enums;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Queries;

public class DbContextBookingQueries(BookingsDbContext dbContext) : IBookingQueries
{
    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Booking?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetByProviderIdPagedAsync(
        Guid providerId, DateOnly? fromDate, DateOnly? toDate, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.ProviderId == providerId);

        return await GetBookingsPagedAsync(query, fromDate, toDate, page, pageSize, cancellationToken);
    }

    public async Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetByClientIdPagedAsync(
        Guid clientId, DateOnly? fromDate, DateOnly? toDate, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.ClientId == clientId);

        return await GetBookingsPagedAsync(query, fromDate, toDate, page, pageSize, cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetActiveByProviderAndDateAsync(
        Guid providerId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.ProviderId == providerId &&
                        b.Date == date &&
                        b.Status != EBookingStatus.Cancelled &&
                        b.Status != EBookingStatus.Rejected &&
                        b.Status != EBookingStatus.Completed)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetByProviderAndPeriodAsync(
        Guid providerId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.ProviderId == providerId && b.Date >= fromDate && b.Date <= toDate)
            .OrderBy(b => b.Date)
            .ThenBy(b => b.TimeSlot.Start)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasCompletedBookingAsync(Guid clientId, Guid providerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings
            .AsNoTracking()
            .AnyAsync(b => b.ClientId == clientId &&
                           b.ProviderId == providerId &&
                           b.Status == EBookingStatus.Completed,
                cancellationToken);
    }

    private static async Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetBookingsPagedAsync(
        IQueryable<Booking> query,
        DateOnly? fromDate,
        DateOnly? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        if (fromDate.HasValue) query = query.Where(b => b.Date >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(b => b.Date <= toDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(b => b.Date)
            .ThenByDescending(b => b.TimeSlot.Start)
            .ThenBy(b => b.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}


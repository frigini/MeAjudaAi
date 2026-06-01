using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Queries;

public class DbContextProviderScheduleQueries(BookingsDbContext dbContext) : IProviderScheduleQueries
{
    public async Task<ProviderSchedule?> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProviderSchedules
            .FirstOrDefaultAsync(ps => ps.ProviderId == providerId, cancellationToken);
    }

    public async Task<ProviderSchedule?> GetByProviderIdReadOnlyAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProviderSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(ps => ps.ProviderId == providerId, cancellationToken);
    }
}

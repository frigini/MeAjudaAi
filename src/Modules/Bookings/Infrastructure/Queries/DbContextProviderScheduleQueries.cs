using MeAjudaAi.Modules.Bookings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Queries;

public class DbContextProviderScheduleQueries(BookingsDbContext _dbContext) : IProviderScheduleQueries
{
    private readonly BookingsDbContext _dbContext = _dbContext ?? throw new ArgumentNullException(nameof(_dbContext));

    public async Task<ProviderSchedule?> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProviderSchedules
            .FirstOrDefaultAsync(ps => ps.ProviderId == providerId, cancellationToken);
    }

    public async Task<ProviderSchedule?> GetByProviderIdReadOnlyAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProviderSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(ps => ps.ProviderId == providerId, cancellationToken);
    }
}

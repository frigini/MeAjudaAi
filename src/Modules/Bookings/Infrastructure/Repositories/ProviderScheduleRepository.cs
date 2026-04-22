using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.Repositories;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Repositories;

public class ProviderScheduleRepository(BookingsDbContext context) : IProviderScheduleRepository
{
    public async Task<ProviderSchedule?> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return await context.ProviderSchedules
            .FirstOrDefaultAsync(ps => ps.ProviderId == providerId, cancellationToken);
    }

    public async Task AddAsync(ProviderSchedule schedule, CancellationToken cancellationToken = default)
    {
        await context.ProviderSchedules.AddAsync(schedule, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ProviderSchedule schedule, CancellationToken cancellationToken = default)
    {
        context.ProviderSchedules.Update(schedule);
        await context.SaveChangesAsync(cancellationToken);
    }
}

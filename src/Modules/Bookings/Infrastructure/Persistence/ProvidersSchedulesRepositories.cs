using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;

public partial class BookingsDbContext : IRepository<ProviderSchedule, Guid>
{
    async Task<ProviderSchedule?> IRepository<ProviderSchedule, Guid>.TryFindAsync(Guid key, CancellationToken cancellationToken) =>
        await ProviderSchedules.FirstOrDefaultAsync(ps => ps.Id == key, cancellationToken);

    void IRepository<ProviderSchedule, Guid>.Add(ProviderSchedule aggregate) => ProviderSchedules.Add(aggregate);
    void IRepository<ProviderSchedule, Guid>.Delete(ProviderSchedule aggregate) => ProviderSchedules.Remove(aggregate);
}

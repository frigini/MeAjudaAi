using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;

public partial class BookingsDbContext : IRepository<Booking, Guid>, IRepository<ProviderSchedule, Guid>
{
    async Task<Booking?> IRepository<Booking, Guid>.TryFindAsync(Guid key, CancellationToken cancellationToken) =>
        await Bookings.FirstOrDefaultAsync(b => b.Id == key, cancellationToken);

    void IRepository<Booking, Guid>.Add(Booking aggregate) => Bookings.Add(aggregate);
    void IRepository<Booking, Guid>.Delete(Booking aggregate) => Bookings.Remove(aggregate);

    async Task<ProviderSchedule?> IRepository<ProviderSchedule, Guid>.TryFindAsync(Guid key, CancellationToken cancellationToken) =>
        await ProviderSchedules.FirstOrDefaultAsync(ps => ps.Id == key, cancellationToken);

    void IRepository<ProviderSchedule, Guid>.Add(ProviderSchedule aggregate) => ProviderSchedules.Add(aggregate);
    void IRepository<ProviderSchedule, Guid>.Delete(ProviderSchedule aggregate) => ProviderSchedules.Remove(aggregate);
}

using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;

public partial class BookingsDbContext : IRepository<Booking, Guid>
{
    async Task<Booking?> IRepository<Booking, Guid>.TryFindAsync(Guid key, CancellationToken cancellationToken) =>
        await Bookings.FirstOrDefaultAsync(b => b.Id == key, cancellationToken);

    void IRepository<Booking, Guid>.Add(Booking aggregate) => Bookings.Add(aggregate);
    void IRepository<Booking, Guid>.Delete(Booking aggregate) => Bookings.Remove(aggregate);

}
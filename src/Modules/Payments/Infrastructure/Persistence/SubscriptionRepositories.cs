using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Persistence;

public partial class PaymentsDbContext : IRepository<Subscription, Guid>
{
    async Task<Subscription?> IRepository<Subscription, Guid>.TryFindAsync(Guid key, CancellationToken cancellationToken) =>
        await Subscriptions.FirstOrDefaultAsync(s => s.Id == key, cancellationToken);

    void IRepository<Subscription, Guid>.Add(Subscription aggregate) => Subscriptions.Add(aggregate);
    void IRepository<Subscription, Guid>.Delete(Subscription aggregate) => Subscriptions.Remove(aggregate);
}



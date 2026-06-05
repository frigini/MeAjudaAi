using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Persistence;

public partial class PaymentsDbContext : IRepository<InboxMessage, Guid>
{
    async Task<InboxMessage?> IRepository<InboxMessage, Guid>.TryFindAsync(Guid key, CancellationToken cancellationToken) =>
        await InboxMessages.FirstOrDefaultAsync(m => m.Id == key, cancellationToken);

    void IRepository<InboxMessage, Guid>.Add(InboxMessage aggregate) => InboxMessages.Add(aggregate);
    void IRepository<InboxMessage, Guid>.Delete(InboxMessage aggregate) => InboxMessages.Remove(aggregate);
}



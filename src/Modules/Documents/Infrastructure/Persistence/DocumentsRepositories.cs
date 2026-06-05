using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Outbox;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Persistence;

public partial class DocumentsDbContext : IRepository<Document, Guid>, IRepository<OutboxMessage, Guid>
{
    public IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>()
    {
        if (typeof(TAggregate) == typeof(Document))
        {
            if (typeof(TKey) != typeof(Guid))
            {
                throw new NotSupportedException($"Repository for Document requires key of type Guid, but {typeof(TKey).Name} was provided.");
            }
            return (IRepository<TAggregate, TKey>)(object)this;
        }
        if (typeof(TAggregate) == typeof(OutboxMessage))
        {
            if (typeof(TKey) != typeof(Guid))
            {
                throw new NotSupportedException($"Repository for OutboxMessage requires key of type Guid, but {typeof(TKey).Name} was provided.");
            }
            return (IRepository<TAggregate, TKey>)(object)this;
        }
        throw new NotSupportedException($"Repository for {typeof(TAggregate).Name} is not supported.");
    }

    async Task<Document?> IRepository<Document, Guid>.TryFindAsync(
        Guid key, CancellationToken ct) =>
        await Documents.FirstOrDefaultAsync(x => x.Id == key, ct);

    void IRepository<Document, Guid>.Add(Document aggregate) =>
        Documents.Add(aggregate);

    void IRepository<Document, Guid>.Delete(Document aggregate) =>
        Documents.Remove(aggregate);

    async Task<OutboxMessage?> IRepository<OutboxMessage, Guid>.TryFindAsync(
        Guid key, CancellationToken ct) =>
        await OutboxMessages.FirstOrDefaultAsync(x => x.Id == key, ct);

    void IRepository<OutboxMessage, Guid>.Add(OutboxMessage aggregate) =>
        OutboxMessages.Add(aggregate);

    void IRepository<OutboxMessage, Guid>.Delete(OutboxMessage aggregate) =>
        OutboxMessages.Remove(aggregate);
}



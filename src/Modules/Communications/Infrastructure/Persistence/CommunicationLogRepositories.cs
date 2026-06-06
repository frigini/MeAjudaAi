using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Persistence;

public partial class CommunicationsDbContext : IRepository<CommunicationLog, Guid>
{
    async Task<CommunicationLog?> IRepository<CommunicationLog, Guid>.TryFindAsync(Guid key, CancellationToken cancellationToken) =>
        await CommunicationLogs.FirstOrDefaultAsync(l => l.Id == key, cancellationToken);

    void IRepository<CommunicationLog, Guid>.Add(CommunicationLog aggregate) => CommunicationLogs.Add(aggregate);
    void IRepository<CommunicationLog, Guid>.Delete(CommunicationLog aggregate) => CommunicationLogs.Remove(aggregate);
}



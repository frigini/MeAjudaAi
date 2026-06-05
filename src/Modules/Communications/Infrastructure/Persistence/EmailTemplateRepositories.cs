using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Persistence;

public partial class CommunicationsDbContext : IRepository<EmailTemplate, Guid>
{
    async Task<EmailTemplate?> IRepository<EmailTemplate, Guid>.TryFindAsync(Guid key, CancellationToken cancellationToken) =>
        await EmailTemplates.FirstOrDefaultAsync(t => t.Id == key, cancellationToken);

    void IRepository<EmailTemplate, Guid>.Add(EmailTemplate aggregate) => EmailTemplates.Add(aggregate);
    void IRepository<EmailTemplate, Guid>.Delete(EmailTemplate aggregate) => EmailTemplates.Remove(aggregate);
}



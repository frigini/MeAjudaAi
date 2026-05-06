using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Persistence;

public partial class DocumentsDbContext : IRepository<Document, DocumentId>
{
    async Task<Document?> IRepository<Document, DocumentId>.TryFindAsync(DocumentId key, CancellationToken ct)
    {
        return await Documents.FirstOrDefaultAsync(x => x.Id == key, ct);
    }

    void IRepository<Document, DocumentId>.Add(Document aggregate)
    {
        Documents.Add(aggregate);
    }

    void IRepository<Document, DocumentId>.Delete(Document aggregate)
    {
        Documents.Remove(aggregate);
    }
}

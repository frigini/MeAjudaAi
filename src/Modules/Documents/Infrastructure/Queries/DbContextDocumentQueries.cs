namespace MeAjudaAi.Modules.Documents.Infrastructure.Queries;

using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class DbContextDocumentQueries(DocumentsDbContext dbContext) : IDocumentQueries
{
    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Documents
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Document>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default) =>
        await dbContext.Documents
            .AsNoTracking()
            .Where(x => x.ProviderId == providerId)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Documents
            .AsNoTracking()
            .AnyAsync(x => x.Id == id, cancellationToken);
}

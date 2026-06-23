using MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Queries;

public class DbContextPaymentsHealthQueries(PaymentsDbContext dbContext) : IPaymentsHealthQueries
{
    public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Database.CanConnectAsync(cancellationToken);
    }
}
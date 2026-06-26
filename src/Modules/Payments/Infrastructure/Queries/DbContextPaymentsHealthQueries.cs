using MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Queries;

public class DbContextPaymentsHealthQueries(PaymentsDbContext dbContext) : IPaymentsHealthQueries
{
    private readonly PaymentsDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Database.CanConnectAsync(cancellationToken);
    }
}
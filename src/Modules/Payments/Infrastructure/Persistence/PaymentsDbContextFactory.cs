using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Persistence;

/// <summary>
/// Factory para criação do PaymentsDbContext em design time (para migrações)
/// O nome do módulo é detectado automaticamente do namespace
/// </summary>
public class PaymentsDbContextFactory : BaseDesignTimeDbContextFactory<PaymentsDbContext>
{
    protected override PaymentsDbContext CreateDbContextInstance(DbContextOptions<PaymentsDbContext> options)
    {
        return new PaymentsDbContext(options);
    }
}

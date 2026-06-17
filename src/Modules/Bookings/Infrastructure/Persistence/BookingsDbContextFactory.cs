using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;

/// <summary>
/// Factory para criação do BookingsDbContext em design time (para migrações)
/// O nome do módulo é detectado automaticamente do namespace
/// </summary>
public class BookingsDbContextFactory : BaseDesignTimeDbContextFactory<BookingsDbContext>
{
    protected override BookingsDbContext CreateDbContextInstance(DbContextOptions<BookingsDbContext> options)
    {
        return new BookingsDbContext(options, null!);
    }
}

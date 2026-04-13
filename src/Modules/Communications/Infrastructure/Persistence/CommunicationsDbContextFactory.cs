using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Persistence;

/// <summary>
/// Factory para criação do DbContext em tempo de design (usado por dotnet ef migrations).
/// </summary>
public class CommunicationsDbContextFactory : BaseDesignTimeDbContextFactory<CommunicationsDbContext>
{
    protected override CommunicationsDbContext CreateDbContextInstance(DbContextOptions<CommunicationsDbContext> options)
    {
        return new CommunicationsDbContext(options);
    }
}

using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Infrastructure;

public static class CommunicationsTestDb
{
    public static CommunicationsDbContext CreateSqlite()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<CommunicationsDbContext>()
            .UseSqlite(connection)
            .UseSnakeCaseNamingConvention()
            .Options;

        var context = new TestCommunicationsDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    private sealed class TestCommunicationsDbContext(DbContextOptions<CommunicationsDbContext> options) : CommunicationsDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var index in entityType.GetIndexes().Where(i => i.GetFilter() != null).ToList())
                {
                    entityType.RemoveIndex(index);
                }
            }
        }
    }
}

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
            .Options;

        var context = new CommunicationsDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}

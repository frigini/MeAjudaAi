using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure;

public static class UsersTestDb
{
    public static UsersDbContext CreateSqlite()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new UsersDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}

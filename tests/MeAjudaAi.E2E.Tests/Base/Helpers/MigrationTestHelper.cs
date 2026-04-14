using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.E2E.Tests.Base.Helpers;

public static class MigrationTestHelper
{
    public static async Task ApplyMigrationForContext(DbContext context)
    {
        var contextName = context.GetType().Name;
        
        // Ensure schema exists before migrating
        var schema = DbContextSchemaHelper.GetSchemaName(contextName);
        if (schema != "public")
        {
            await context.Database.ExecuteSqlRawAsync($"CREATE SCHEMA IF NOT EXISTS \"{schema}\";");
        }
        
        await context.Database.MigrateAsync();
    }
}

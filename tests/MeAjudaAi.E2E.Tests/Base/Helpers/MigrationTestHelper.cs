using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.E2E.Tests.Base.Helpers;

public static class MigrationTestHelper
{
    public static async Task ApplyMigrationForContext(DbContext context)
    {
        var contextName = context.GetType().Name;
        var maxRetries = 5;
        var delayMs = 2000;
        
        Console.WriteLine($"🔄 [MigrationTestHelper] Applying migrations for {contextName}...");

        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                // Tentar abrir conexão primeiro com timeout reduzido
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                await context.Database.OpenConnectionAsync(cts.Token);
                await context.Database.CloseConnectionAsync();
                
                // Garantir que o esquema exista antes da migração
                var schema = DbContextSchemaHelper.GetSchemaName(contextName);
                if (schema != "public")
                {
                    await context.Database.ExecuteSqlRawAsync($"CREATE SCHEMA IF NOT EXISTS \"{schema}\";", cts.Token);
                }
                
                await context.Database.MigrateAsync(cts.Token);
                Console.WriteLine($"✅ [MigrationTestHelper] Migrations for {contextName} applied.");
                return;
            }
            catch (Exception ex) when (attempt < maxRetries - 1)
            {
                Console.WriteLine($"⚠️ [MigrationTestHelper] Migration attempt {attempt + 1} for {contextName} failed: {ex.Message}. Retrying in {delayMs}ms...");
                await Task.Delay(delayMs);
                delayMs *= 2;
            }
        }
        
        // Última tentativa - deixe o erro original propagate
        try
        {
            var schema = DbContextSchemaHelper.GetSchemaName(contextName);
            if (schema != "public")
            {
                await context.Database.ExecuteSqlRawAsync($"CREATE SCHEMA IF NOT EXISTS \"{schema}\";");
            }
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to apply migrations for {contextName} after {maxRetries} attempts", ex);
        }
    }
}

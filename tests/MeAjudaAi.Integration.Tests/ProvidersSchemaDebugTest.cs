using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests;

/// <summary>
/// 🧪 TESTE DIAGNÓSTICO PARA SCHEMA PROVIDERS
/// 
/// Verifica exatamente o que acontece com o schema providers
/// </summary>
public class ProvidersSchemaDebugTest(ITestOutputHelper testOutput) : ApiTestBase
{
    [Fact]
    public async Task Debug_Providers_Schema_Creation()
    {
        // Arrange
        var dbContext = Services.GetRequiredService<ProvidersDbContext>();

        try
        {
            // Act: Check if providers schema exists
            var schemasQuery = @"
                SELECT schema_name 
                FROM information_schema.schemata 
                WHERE schema_name NOT IN ('information_schema', 'pg_catalog', 'pg_toast')
                ORDER BY schema_name";

            using var connection = dbContext.Database.GetDbConnection();
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = schemasQuery;
            using var reader = await command.ExecuteReaderAsync();

            var schemas = new List<string>();
            while (await reader.ReadAsync())
            {
                schemas.Add(reader.GetString(0));
            }

            testOutput.WriteLine($"Available schemas: {string.Join(", ", schemas)}");
            await reader.DisposeAsync();

            // Check if providers table exists in any schema
            var allTablesQuery = @"
                SELECT table_schema, table_name 
                FROM information_schema.tables 
                WHERE table_name LIKE '%provider%'
                ORDER BY table_schema, table_name";

            command.CommandText = allTablesQuery;
            using var tablesReader = await command.ExecuteReaderAsync();

            var tables = new List<string>();
            while (await tablesReader.ReadAsync())
            {
                tables.Add($"{tablesReader.GetString(0)}.{tablesReader.GetString(1)}");
            }

            testOutput.WriteLine($"Provider-related tables: {string.Join(", ", tables)}");
            await connection.CloseAsync();

            // Assert: Validate that expected schema and tables exist
            schemas.Should().Contain("providers", "The 'providers' schema should exist");
            tables.Should().NotBeEmpty("At least one provider-related table should exist");
            tables.Should().Contain(t => t.Contains("providers.providers"), "The main providers table should exist in the providers schema");
        }
        catch (Exception ex)
        {
            testOutput.WriteLine($"Error during debug: {ex.Message}");
            throw; // Re-throw to fail the test when DB state is unexpected
        }
    }
}

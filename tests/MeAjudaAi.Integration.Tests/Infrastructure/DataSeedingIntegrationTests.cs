using FluentAssertions;
using MeAjudaAi.Integration.Tests.Aspire;
using Npgsql;

namespace MeAjudaAi.Integration.Tests.Infrastructure;

/// <summary>
/// Testes de integração para validar data seeding via SQL scripts.
/// Valida que os seeds em infrastructure/database/seeds/ são executados corretamente.
/// 
/// KNOWN ISSUE: Aspire DCP binaries não encontrados no CI (.NET 10)
/// Workload deprecado - binários devem vir de NuGet packages mas path não está configurado.
/// Tracked in: https://github.com/dotnet/aspire/issues
/// </summary>
[Trait("Category", "Integration")]
[Trait("Area", "Infrastructure")]
[Trait("Category", "Aspire")]
[Trait("Issue", "AspireDCP")]
public sealed class DataSeedingIntegrationTests(AspireIntegrationFixture _) 
    : IClassFixture<AspireIntegrationFixture>
{
    private const string ServiceCatalogsSchema = "meajudaai_service_catalogs";

    #region ServiceCatalogs Seeding Tests

    [Fact(Skip = "KNOWN ISSUE: Aspire DCP binaries not found - .NET 10 workload deprecation")]
    public async Task ServiceCatalogs_ShouldHave8Categories()
    {
        // Arrange
        var connectionString = GetConnectionString();

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(*) FROM meajudaai_service_catalogs.\"ServiceCategories\"",
            connection);
        
        var count = (long)(await command.ExecuteScalarAsync() ?? 0L);

        // Assert
        count.Should().Be(8, "deve haver 8 categorias de serviço no seed");
    }

    [Fact(Skip = "KNOWN ISSUE: Aspire DCP binaries not found - .NET 10 workload deprecation")]
    public async Task ServiceCatalogs_ShouldHaveExpectedCategories()
    {
        // Arrange
        var connectionString = GetConnectionString();

        var expectedCategories = new[]
        {
            "Saúde",
            "Educação",
            "Assistência Social",
            "Jurídico",
            "Habitação",
            "Transporte",
            "Alimentação",
            "Trabalho e Renda"
        };

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(
            $"SELECT \"Name\" FROM {ServiceCatalogsSchema}.\"ServiceCategories\"",
            connection);
        
        await using var reader = await command.ExecuteReaderAsync();
        var categories = new List<string>();
        
        while (await reader.ReadAsync())
        {
            categories.Add(reader.GetString(0));
        }

        // Assert - using BeEquivalentTo for unordered comparison
        categories.Should().BeEquivalentTo(expectedCategories);
    }

    [Fact(Skip = "KNOWN ISSUE: Aspire DCP binaries not found - .NET 10 workload deprecation")]
    public async Task ServiceCatalogs_ShouldHave12Services()
    {
        // Arrange
        var connectionString = GetConnectionString();

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(
            $"SELECT COUNT(*) FROM {ServiceCatalogsSchema}.\"Services\"",
            connection);
        
        var count = (long)(await command.ExecuteScalarAsync() ?? 0L);

        // Assert
        count.Should().Be(12, "deve haver 12 serviços no seed");
    }

    [Fact(Skip = "KNOWN ISSUE: Aspire DCP binaries not found - .NET 10 workload deprecation")]
    public async Task ServiceCatalogs_AllServicesLinkedToCategories()
    {
        // Arrange
        var connectionString = GetConnectionString();

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        // Verificar que todos os serviços têm categoria válida
        await using var command = new NpgsqlCommand(
            $@"SELECT COUNT(*) 
              FROM {ServiceCatalogsSchema}.""Services"" s
              LEFT JOIN {ServiceCatalogsSchema}.""ServiceCategories"" sc ON s.""CategoryId"" = sc.""Id""
              WHERE sc.""Id"" IS NULL",
            connection);
        
        var orphanCount = (long)(await command.ExecuteScalarAsync() ?? 0L);

        // Assert
        orphanCount.Should().Be(0, "todos os serviços devem estar vinculados a categorias válidas");
    }

    [Fact(Skip = "KNOWN ISSUE: Aspire DCP binaries not found - .NET 10 workload deprecation")]
    public async Task ServiceCatalogs_IdempotencyCheck_RunningTwiceShouldNotDuplicate()
    {
        // Arrange
        var connectionString = GetConnectionString();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Garantir estado limpo para o nome usado no teste
        // TODO: Consider using Guid.NewGuid() suffix for test isolation in parallel execution
        await using (var cleanupBefore = new NpgsqlCommand(
            $"DELETE FROM {ServiceCatalogsSchema}.\"ServiceCategories\" WHERE \"Name\" = 'Teste Idempotência'",
            connection))
        {
            await cleanupBefore.ExecuteNonQueryAsync();
        }

        // Contar registros antes
        await using var countBefore = new NpgsqlCommand(
            $"SELECT COUNT(*) FROM {ServiceCatalogsSchema}.\"ServiceCategories\"",
            connection);
        var countBeforeExec = (long)(await countBefore.ExecuteScalarAsync() ?? 0L);

        // Act - Tentar executar seed novamente (simula idempotência)
        // TODO: Consider loading and executing actual seed script from infrastructure/database/seeds/01-seed-service-catalogs.sql
        // for more accurate validation. For now, validating the idempotency pattern with inline SQL:
        var idempotentSql = $@"DO $$
              BEGIN
                  -- Script idempotente: deve verificar se já existe antes de inserir
                  IF NOT EXISTS (SELECT 1 FROM {ServiceCatalogsSchema}.""ServiceCategories"" WHERE ""Name"" = 'Teste Idempotência') THEN
                      INSERT INTO {ServiceCatalogsSchema}.""ServiceCategories"" (""Id"", ""Name"", ""Description"", ""Icon"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
                      VALUES (gen_random_uuid(), 'Teste Idempotência', 'Test', 'test', true, NOW(), NOW());
                  END IF;
              END $$;";

        // Execute twice to verify idempotency - should only insert once
        for (int i = 0; i < 2; i++)
        {
            await using var rerunSeed = new NpgsqlCommand(idempotentSql, connection);
            await rerunSeed.ExecuteNonQueryAsync();
        }

        await using var countAfter = new NpgsqlCommand(
            $"SELECT COUNT(*) FROM {ServiceCatalogsSchema}.\"ServiceCategories\"",
            connection);
        var countAfterExec = (long)(await countAfter.ExecuteScalarAsync() ?? 0L);

        try
        {
            // Assert - deve ter apenas 1 registro a mais (não 2)
            countAfterExec.Should().Be(countBeforeExec + 1, "idempotência deve prevenir duplicação");
        }
        finally
        {
            // Cleanup
            await using var cleanup = new NpgsqlCommand(
                $"DELETE FROM {ServiceCatalogsSchema}.\"ServiceCategories\" WHERE \"Name\" = 'Teste Idempotência'",
                connection);
            await cleanup.ExecuteNonQueryAsync();
        }
    }

    [Fact(Skip = "KNOWN ISSUE: Aspire DCP binaries not found - .NET 10 workload deprecation")]
    public async Task ServiceCatalogs_ShouldHaveSpecificServices()
    {
        // Arrange
        var connectionString = GetConnectionString();

        var expectedServices = new[]
        {
            "Consulta Médica Geral",
            "Atendimento de Urgência e Emergência",
            "Educação Infantil (Creche)",
            "Ensino Fundamental",
            "Auxílio Alimentação (Cesta Básica)",
            "Restaurante Popular",
            "Orientação Jurídica Gratuita",
            "Atendimento do Centro de Referência de Assistência Social (CRAS)",
            "Cadastro Habitacional",
            "Transporte Público Subsidiado",
            "Encaminhamento para Vagas de Emprego",
            "Cursos de Qualificação Profissional"
        };

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(
            $"SELECT \"Name\" FROM {ServiceCatalogsSchema}.\"Services\"",
            connection);
        
        await using var reader = await command.ExecuteReaderAsync();
        var services = new List<string>();
        
        while (await reader.ReadAsync())
        {
            services.Add(reader.GetString(0));
        }

        // Assert - using BeEquivalentTo for unordered comparison
        services.Should().BeEquivalentTo(expectedServices);
    }

    [Fact(Skip = "KNOWN ISSUE: Aspire DCP binaries not found - .NET 10 workload deprecation")]
    public async Task ServiceCatalogs_AllCategoriesAreActive()
    {
        // Arrange
        var connectionString = GetConnectionString();

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(
            $"SELECT COUNT(*) FROM {ServiceCatalogsSchema}.\"ServiceCategories\" WHERE \"IsActive\" = false",
            connection);
        
        var inactiveCount = (long)(await command.ExecuteScalarAsync() ?? 0L);

        // Assert
        inactiveCount.Should().Be(0, "todas as categorias do seed devem estar ativas");
    }

    [Fact(Skip = "KNOWN ISSUE: Aspire DCP binaries not found - .NET 10 workload deprecation")]
    public async Task ServiceCatalogs_AllServicesAreActive()
    {
        // Arrange
        var connectionString = GetConnectionString();

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(
            $"SELECT COUNT(*) FROM {ServiceCatalogsSchema}.\"Services\" WHERE \"IsActive\" = false",
            connection);
        
        var inactiveCount = (long)(await command.ExecuteScalarAsync() ?? 0L);

        // Assert
        inactiveCount.Should().Be(0, "todos os serviços do seed devem estar ativos");
    }

    #endregion

    #region Helper Methods

    private string GetConnectionString()
    {
        // Prefer Aspire-injected connection string from orchestrated services
        // (e.g., "ConnectionStrings__postgresdb" when using WithReference in AppHost)
        var aspireConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__postgresdb");
        if (!string.IsNullOrWhiteSpace(aspireConnectionString))
        {
            return aspireConnectionString;
        }

        // NOTE: Using fallback connection string - Aspire orchestration may not be active
        // Fallback to custom environment variables for CI/CD or local testing without Aspire
        var host = Environment.GetEnvironmentVariable("MEAJUDAAI_DB_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("MEAJUDAAI_DB_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("MEAJUDAAI_DB") ?? "meajudaai_tests";
        var username = Environment.GetEnvironmentVariable("MEAJUDAAI_DB_USER") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("MEAJUDAAI_DB_PASS") ?? "postgres";

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }

    #endregion
}

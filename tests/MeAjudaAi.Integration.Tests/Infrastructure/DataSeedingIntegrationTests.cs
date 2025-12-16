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
// Note: AspireIntegrationFixture is required to ensure Aspire AppHost orchestration is active
// and database services are available, even though tests create their own NpgsqlConnection.
// The fixture manages the lifecycle of orchestrated services (PostgreSQL via Aspire WithReference).
#pragma warning disable CS9113 // Parameter is unread but required for IClassFixture<> lifecycle
public sealed class DataSeedingIntegrationTests(AspireIntegrationFixture _) 
    : IClassFixture<AspireIntegrationFixture>
#pragma warning restore CS9113
{
    private const string ServiceCatalogsSchema = "meajudaai_service_catalogs";

    #region ServiceCatalogs Seeding Tests

    [Fact]
    public async Task ServiceCatalogs_ShouldHave8Categories()
    {
        // Arrange
        var connectionString = GetConnectionString();

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        // NOTE: String interpolation is safe here - ServiceCatalogsSchema is a compile-time constant
        // (not user input), so no SQL injection risk. Using interpolation for better readability.
        await using var command = new NpgsqlCommand(
            $"SELECT COUNT(*) FROM {ServiceCatalogsSchema}.\"ServiceCategories\"",
            connection);
        
        var count = (long)(await command.ExecuteScalarAsync() ?? 0L);

        // Assert
        count.Should().Be(8, "deve haver 8 categorias de serviço no seed");
    }

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
    public async Task ServiceCatalogs_IdempotencyCheck_RunningTwiceShouldNotDuplicate()
    {
        // Arrange
        var connectionString = GetConnectionString();
        var testGuid = Guid.NewGuid().ToString("N")[..8]; // Use unique ID for parallel test isolation
        var testCategoryName = $"Teste Idempotência {testGuid}";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Garantir estado limpo para o nome usado no teste
        await using (var cleanupBefore = new NpgsqlCommand(
            $"DELETE FROM {ServiceCatalogsSchema}.\"ServiceCategories\" WHERE \"Name\" = @name",
            connection))
        {
            cleanupBefore.Parameters.AddWithValue("name", testCategoryName);
            await cleanupBefore.ExecuteNonQueryAsync();
        }

        // Contar registros antes
        await using var countBefore = new NpgsqlCommand(
            $"SELECT COUNT(*) FROM {ServiceCatalogsSchema}.\"ServiceCategories\"",
            connection);
        var countBeforeExec = (long)(await countBefore.ExecuteScalarAsync() ?? 0L);

        // Act - Tentar executar seed novamente (simula idempotência)
        // Validating the idempotency pattern: should check if exists before inserting
        // Note: Using string interpolation for testCategoryName since DO blocks don't support parameters
        // testCategoryName is a controlled GUID-based string, not user input - no SQL injection risk
        var idempotentSql = $@"DO $$
              BEGIN
                  -- Script idempotente: deve verificar se já existe antes de inserir
                  IF NOT EXISTS (SELECT 1 FROM {ServiceCatalogsSchema}.""ServiceCategories"" WHERE ""Name"" = '{testCategoryName}') THEN
                      INSERT INTO {ServiceCatalogsSchema}.""ServiceCategories"" (""Id"", ""Name"", ""Description"", ""Icon"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
                      VALUES (gen_random_uuid(), '{testCategoryName}', 'Test', 'test', true, NOW(), NOW());
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
                $"DELETE FROM {ServiceCatalogsSchema}.\"ServiceCategories\" WHERE \"Name\" = @name",
                connection);
            cleanup.Parameters.AddWithValue("name", testCategoryName);
            await cleanup.ExecuteNonQueryAsync();
        }
    }

    [Fact]
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

    [Fact]
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

    [Fact]
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

        // Fallback: Use CI/local environment variables
        // CI workflow sets MEAJUDAAI_DB_* vars; local dev can use defaults or override
        var host = Environment.GetEnvironmentVariable("MEAJUDAAI_DB_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("MEAJUDAAI_DB_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("MEAJUDAAI_DB") ?? "meajudaai_tests";
        var username = Environment.GetEnvironmentVariable("MEAJUDAAI_DB_USER") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("MEAJUDAAI_DB_PASS") ?? "postgres";

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }

    #endregion
}

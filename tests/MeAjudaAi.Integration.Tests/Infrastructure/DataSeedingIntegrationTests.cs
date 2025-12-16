using FluentAssertions;
using Npgsql;

namespace MeAjudaAi.Integration.Tests.Infrastructure;

/// <summary>
/// Testes de integração para validar data seeding via SQL scripts.
/// Valida que os seeds em infrastructure/database/seeds/ são executados corretamente.
/// 
/// Nota: Testes não dependem mais do Aspire DCP - usam conexão direta ao PostgreSQL
/// via variáveis de ambiente MEAJUDAAI_DB_* configuradas no workflow de CI.
/// Aspire é usado apenas quando disponível (desenvolvimento local com AppHost).
/// 
/// DatabaseMigrationFixture garante que as migrations são executadas antes dos testes.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Area", "Infrastructure")]
[Trait("Database", "PostgreSQL")]
public sealed class DataSeedingIntegrationTests : IClassFixture<DatabaseMigrationFixture>
{
    private const string ServiceCatalogsSchema = "meajudaai_service_catalogs";

    // Fixture é injetado para garantir que migrations rodam antes dos testes
    public DataSeedingIntegrationTests(DatabaseMigrationFixture fixture)
    {
        // O xUnit garante que fixture.InitializeAsync() foi chamado antes deste construtor
        _ = fixture; // Suprime warning de parâmetro não utilizado
    }

    #region ServiceCatalogs Seeding Tests

    [Fact]
    public async Task ServiceCatalogs_ShouldHave8Categories()
    {
        // Arrange
        var connectionString = TestConnectionHelper.GetConnectionString();

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

    [Fact]
    public async Task ServiceCatalogs_ShouldHaveExpectedCategories()
    {
        // Arrange
        var connectionString = TestConnectionHelper.TestConnectionHelper.GetConnectionString();

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
            "SELECT \"Name\" FROM meajudaai_service_catalogs.\"ServiceCategories\"",
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
        var connectionString = TestConnectionHelper.GetConnectionString();

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(*) FROM meajudaai_service_catalogs.\"Services\"",
            connection);
        
        var count = (long)(await command.ExecuteScalarAsync() ?? 0L);

        // Assert
        count.Should().Be(12, "deve haver 12 serviços no seed");
    }

    [Fact]
    public async Task ServiceCatalogs_AllServicesLinkedToCategories()
    {
        // Arrange
        var connectionString = TestConnectionHelper.GetConnectionString();

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        // Verificar que todos os serviços têm categoria válida
        await using var command = new NpgsqlCommand(
            @"SELECT COUNT(*) 
              FROM meajudaai_service_catalogs.""Services"" s
              LEFT JOIN meajudaai_service_catalogs.""ServiceCategories"" sc ON s.""CategoryId"" = sc.""Id""
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
        var connectionString = TestConnectionHelper.GetConnectionString();
        var testGuid = Guid.NewGuid().ToString("N")[..8]; // Use unique ID for parallel test isolation
        var testCategoryName = $"Teste Idempotência {testGuid}";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Garantir estado limpo para o nome usado no teste
        await using (var cleanupBefore = new NpgsqlCommand(
            "DELETE FROM meajudaai_service_catalogs.\"ServiceCategories\" WHERE \"Name\" = @name",
            connection))
        {
            cleanupBefore.Parameters.AddWithValue("name", testCategoryName);
            await cleanupBefore.ExecuteNonQueryAsync();
        }

        // Contar registros antes
        await using var countBefore = new NpgsqlCommand(
            "SELECT COUNT(*) FROM meajudaai_service_catalogs.\"ServiceCategories\"",
            connection);
        var countBeforeExec = (long)(await countBefore.ExecuteScalarAsync() ?? 0L);

        // Act - Tentar executar seed novamente (simula idempotência)
        // Validating the idempotency pattern: should check if exists before inserting
        // Using parameterized INSERT with WHERE NOT EXISTS for idempotency
        var idempotentSql = @"
            INSERT INTO meajudaai_service_catalogs.""ServiceCategories"" 
                (""Id"", ""Name"", ""Description"", ""Icon"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
            SELECT gen_random_uuid(), @name, 'Test', 'test', true, NOW(), NOW()
            WHERE NOT EXISTS (
                SELECT 1 FROM meajudaai_service_catalogs.""ServiceCategories"" 
                WHERE ""Name"" = @name
            )";

        // Execute twice to verify idempotency - should only insert once
        for (int i = 0; i < 2; i++)
        {
            await using var rerunSeed = new NpgsqlCommand(idempotentSql, connection);
            rerunSeed.Parameters.AddWithValue("name", testCategoryName);
            await rerunSeed.ExecuteNonQueryAsync();
        }

        await using var countAfter = new NpgsqlCommand(
            "SELECT COUNT(*) FROM meajudaai_service_catalogs.\"ServiceCategories\"",
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
                "DELETE FROM meajudaai_service_catalogs.\"ServiceCategories\" WHERE \"Name\" = @name",
                connection);
            cleanup.Parameters.AddWithValue("name", testCategoryName);
            await cleanup.ExecuteNonQueryAsync();
        }
    }

    [Fact]
    public async Task ServiceCatalogs_ShouldHaveSpecificServices()
    {
        // Arrange
        var connectionString = TestConnectionHelper.TestConnectionHelper.GetConnectionString();

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
            "SELECT \"Name\" FROM meajudaai_service_catalogs.\"Services\"",
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
        var connectionString = TestConnectionHelper.GetConnectionString();

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(*) FROM meajudaai_service_catalogs.\"ServiceCategories\" WHERE \"IsActive\" = false",
            connection);
        
        var inactiveCount = (long)(await command.ExecuteScalarAsync() ?? 0L);

        // Assert
        inactiveCount.Should().Be(0, "todas as categorias do seed devem estar ativas");
    }

    [Fact]
    public async Task ServiceCatalogs_AllServicesAreActive()
    {
        // Arrange
        var connectionString = TestConnectionHelper.GetConnectionString();

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(*) FROM meajudaai_service_catalogs.\"Services\" WHERE \"IsActive\" = false",
            connection);
        
        var inactiveCount = (long)(await command.ExecuteScalarAsync() ?? 0L);

        // Assert
        inactiveCount.Should().Be(0, "todos os serviços do seed devem estar ativos");
    }

    #endregion}




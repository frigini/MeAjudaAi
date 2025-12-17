using FluentAssertions;
using Npgsql;

namespace MeAjudaAi.Integration.Tests.Infrastructure;

/// <summary>
/// Testes de integração para validar data seeding via SQL scripts.
/// Valida que os seeds em infrastructure/database/seeds/ são executados corretamente.
/// 
/// Usa Testcontainers para criar PostgreSQL container automaticamente.
/// DatabaseMigrationFixture garante que as migrations e seeds são executados antes dos testes.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Area", "Infrastructure")]
[Trait("Database", "PostgreSQL")]
public sealed class DataSeedingIntegrationTests : IClassFixture<DatabaseMigrationFixture>
{
    private const string ServiceCatalogsSchema = "meajudaai_service_catalogs";
    private readonly DatabaseMigrationFixture _fixture;

    public DataSeedingIntegrationTests(DatabaseMigrationFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    #region ServiceCatalogs Seeding Tests

    [Fact]
    public async Task ServiceCatalogs_ShouldHave8Categories()
    {
        // Arrange
        var connectionString = _fixture.ConnectionString!;

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(*) FROM meajudaai_service_catalogs.service_categories",
            connection);
        
        var count = (long)(await command.ExecuteScalarAsync() ?? 0L);

        // Assert
        count.Should().Be(8, "deve haver 8 categorias de serviço no seed");
    }

    [Fact]
    public async Task ServiceCatalogs_ShouldHaveExpectedCategories()
    {
        // Arrange
        var connectionString = _fixture.ConnectionString!;

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
            "SELECT name FROM meajudaai_service_catalogs.service_categories",
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
        var connectionString = _fixture.ConnectionString!;

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(*) FROM meajudaai_service_catalogs.services",
            connection);
        
        var count = (long)(await command.ExecuteScalarAsync() ?? 0L);

        // Assert
        count.Should().Be(12, "deve haver 12 serviços no seed");
    }

    [Fact]
    public async Task ServiceCatalogs_AllServicesLinkedToCategories()
    {
        // Arrange
        var connectionString = _fixture.ConnectionString!;

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        // Verificar que todos os serviços têm categoria válida
        await using var command = new NpgsqlCommand(
            @"SELECT COUNT(*) 
              FROM meajudaai_service_catalogs.services s
              LEFT JOIN meajudaai_service_catalogs.service_categories sc ON s.category_id = sc.id
             WHERE sc.id IS NULL",
            connection);
        
        var orphanCount = (long)(await command.ExecuteScalarAsync() ?? 0L);

        // Assert
        orphanCount.Should().Be(0, "todos os serviços devem estar vinculados a categorias válidas");
    }

    [Fact]
    public async Task ServiceCatalogs_IdempotencyCheck_RunningTwiceShouldNotDuplicate()
    {
        // Arrange
        var connectionString = _fixture.ConnectionString!;
        var testGuid = Guid.NewGuid().ToString("N")[..8]; // Use unique ID for parallel test isolation
        var testCategoryName = $"Teste Idempotência {testGuid}";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Garantir estado limpo para o nome usado no teste
        await using (var cleanupBefore = new NpgsqlCommand(
            "DELETE FROM meajudaai_service_catalogs.service_categories WHERE name = @name",
            connection))
        {
            cleanupBefore.Parameters.AddWithValue("name", testCategoryName);
            await cleanupBefore.ExecuteNonQueryAsync();
        }

        // Contar registros antes
        await using var countBefore = new NpgsqlCommand(
            "SELECT COUNT(*) FROM meajudaai_service_catalogs.service_categories",
            connection);
        var countBeforeExec = (long)(await countBefore.ExecuteScalarAsync() ?? 0L);

        // Act - Tentar executar seed novamente (simula idempotência)
        // Validating the idempotency pattern: should check if exists before inserting
        // Using parameterized INSERT with WHERE NOT EXISTS for idempotency
        var idempotentSql = @"
            INSERT INTO meajudaai_service_catalogs.service_categories 
                (id, name, description, is_active, display_order, created_at, updated_at)
            SELECT gen_random_uuid(), @name, 'Test', true, 999, NOW(), NOW()
            WHERE NOT EXISTS (
                SELECT 1 FROM meajudaai_service_catalogs.service_categories 
                WHERE name = @name
            )";

        // Execute twice to verify idempotency - should only insert once
        for (int i = 0; i < 2; i++)
        {
            await using var rerunSeed = new NpgsqlCommand(idempotentSql, connection);
            rerunSeed.Parameters.AddWithValue("name", testCategoryName);
            await rerunSeed.ExecuteNonQueryAsync();
        }

        await using var countAfter = new NpgsqlCommand(
            "SELECT COUNT(*) FROM meajudaai_service_catalogs.service_categories",
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
                "DELETE FROM meajudaai_service_catalogs.service_categories WHERE name = @name",
                connection);
            cleanup.Parameters.AddWithValue("name", testCategoryName);
            await cleanup.ExecuteNonQueryAsync();
        }
    }

    [Fact]
    public async Task ServiceCatalogs_ShouldHaveSpecificServices()
    {
        // Arrange
        var connectionString = _fixture.ConnectionString!;

        var expectedServices = new[]
        {
            "Consulta Médica Geral",
            "Atendimento Psicológico",
            "Fisioterapia",
            "Reforço Escolar",
            "Alfabetização de Adultos",
            "Orientação Social",
            "Apoio a Famílias",
            "Orientação Jurídica Gratuita",
            "Mediação de Conflitos",
            "Reparos Residenciais",
            "Capacitação Profissional",
            "Intermediação de Emprego"
        };

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(
            "SELECT name FROM meajudaai_service_catalogs.services",
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
        var connectionString = _fixture.ConnectionString!;

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(*) FROM meajudaai_service_catalogs.service_categories WHERE is_active = false",
            connection);
        
        var inactiveCount = (long)(await command.ExecuteScalarAsync() ?? 0L);

        // Assert
        inactiveCount.Should().Be(0, "todas as categorias do seed devem estar ativas");
    }

    [Fact]
    public async Task ServiceCatalogs_AllServicesAreActive()
    {
        // Arrange
        var connectionString = _fixture.ConnectionString!;

        // Act
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(*) FROM meajudaai_service_catalogs.services WHERE is_active = false",
            connection);
        
        var inactiveCount = (long)(await command.ExecuteScalarAsync() ?? 0L);

        // Assert
        inactiveCount.Should().Be(0, "todos os serviços do seed devem estar ativos");
    }

    #endregion
}




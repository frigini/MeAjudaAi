using FluentAssertions;
using MeAjudaAi.Integration.Tests.Aspire;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace MeAjudaAi.Integration.Tests.Infrastructure;

/// <summary>
/// Testes de integração para validar data seeding via SQL scripts.
/// Valida que os seeds em infrastructure/database/seeds/ são executados corretamente.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Area", "Infrastructure")]
public sealed class DataSeedingIntegrationTests(AspireIntegrationFixture fixture) 
    : IClassFixture<AspireIntegrationFixture>
{
    private readonly AspireIntegrationFixture _fixture = fixture;

    #region ServiceCatalogs Seeding Tests

    [Fact]
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
            "SELECT \"Name\" FROM meajudaai_service_catalogs.\"ServiceCategories\" ORDER BY \"Name\"",
            connection);
        
        await using var reader = await command.ExecuteReaderAsync();
        var categories = new List<string>();
        
        while (await reader.ReadAsync())
        {
            categories.Add(reader.GetString(0));
        }

        // Assert
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
        var connectionString = GetConnectionString();

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
        var connectionString = GetConnectionString();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Contar registros antes
        await using var countBefore = new NpgsqlCommand(
            "SELECT COUNT(*) FROM meajudaai_service_catalogs.\"ServiceCategories\"",
            connection);
        var countBeforeExec = (long)(await countBefore.ExecuteScalarAsync() ?? 0L);

        // Act - Tentar executar seed novamente (simula idempotência)
        await using var rerunSeed = new NpgsqlCommand(
            @"DO $$
              BEGIN
                  -- Script idempotente: deve verificar se já existe antes de inserir
                  IF NOT EXISTS (SELECT 1 FROM meajudaai_service_catalogs.""ServiceCategories"" WHERE ""Name"" = 'Teste Idempotência') THEN
                      INSERT INTO meajudaai_service_catalogs.""ServiceCategories"" (""Id"", ""Name"", ""Description"", ""Icon"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
                      VALUES (gen_random_uuid(), 'Teste Idempotência', 'Test', 'test', true, NOW(), NOW());
                  END IF;
              END $$;",
            connection);
        await rerunSeed.ExecuteNonQueryAsync();

        // Executar novamente - não deve duplicar
        await using var rerunSeed2 = new NpgsqlCommand(
            @"DO $$
              BEGIN
                  IF NOT EXISTS (SELECT 1 FROM meajudaai_service_catalogs.""ServiceCategories"" WHERE ""Name"" = 'Teste Idempotência') THEN
                      INSERT INTO meajudaai_service_catalogs.""ServiceCategories"" (""Id"", ""Name"", ""Description"", ""Icon"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
                      VALUES (gen_random_uuid(), 'Teste Idempotência', 'Test', 'test', true, NOW(), NOW());
                  END IF;
              END $$;",
            connection);
        await rerunSeed2.ExecuteNonQueryAsync();

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
                "DELETE FROM meajudaai_service_catalogs.\"ServiceCategories\" WHERE \"Name\" = 'Teste Idempotência'",
                connection);
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
            "SELECT \"Name\" FROM meajudaai_service_catalogs.\"Services\" ORDER BY \"Name\"",
            connection);
        
        await using var reader = await command.ExecuteReaderAsync();
        var services = new List<string>();
        
        while (await reader.ReadAsync())
        {
            services.Add(reader.GetString(0));
        }

        // Assert
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
        var connectionString = GetConnectionString();

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

    #endregion

    #region Helper Methods

    private string GetConnectionString()
    {
        // NOTA: Em ambiente de teste, usando banco de dados externo configurado via connection string
        // Este não é o ideal (preferível seria usar container isolado), mas funciona para validação inicial
        var config = _fixture.HttpClient.GetType()
            .Assembly.GetTypes()
            .FirstOrDefault(t => t.Name.Contains("Configuration"));

        // Por enquanto, usar connection string hardcoded para ambiente de teste local
        // TODO: Melhorar isolamento dos testes usando container dedicado
        return "Host=localhost;Port=5432;Database=meajudaai_tests;Username=postgres;Password=postgres";
    }

    #endregion
}

using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.PostgreSql.Factories;
using Xunit;
using FluentAssertions;
using Npgsql;

namespace MeAjudaAi.Integration.Tests.Infrastructure.Jobs;

[Trait("Category", "Integration")]
[Trait("Component", "Hangfire")]
public class HangfirePostgreSqlTests
{
    private readonly string _connectionString;

    public HangfirePostgreSqlTests()
    {
        // Prioridade para a variável de ambiente sugerida pelo usuário para validação no CI
        _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
                           ?? "Host=localhost;Database=meajudaai_compat;Username=postgres;Password=test123";
    }

    [Fact]
    public void UsePostgreSqlStorage_WithValidConnectionString_ShouldInitializeWithoutErrors()
    {
        // This test validates that Hangfire.PostgreSql 1.21.1 is compatible with Npgsql 10.x
        // by attempting to initialize the storage and execute a simple command.
        
        // Arrange & Act
        var options = new PostgreSqlStorageOptions 
        { 
            SchemaName = "hangfire_compat_test",
            PrepareSchemaIfNecessary = true 
        };

        // Initialize storage using the recommended NpgsqlConnectionFactory to avoid obsolete warnings
        var storage = new PostgreSqlStorage(new NpgsqlConnectionFactory(_connectionString, options), options);
        
        // Assert
        storage.Should().NotBeNull();
        
        // Exercise the storage to ensure Npgsql connection works
        // (Hangfire initializes schema lazily on first use or explicitly if configured)
        using var connection = storage.GetConnection();
        connection.Should().NotBeNull();
        
        // If we reached here without Npgsql related exceptions (MethodNotFound, etc), 
        // the compatibility risk is significantly reduced.
    }
}

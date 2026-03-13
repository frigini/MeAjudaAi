using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.PostgreSql.Factories;
using Xunit;
using FluentAssertions;
using Npgsql;
using MeAjudaAi.Integration.Tests.Infrastructure;

namespace MeAjudaAi.Integration.Tests.Infrastructure.Jobs;

[Trait("Category", "Integration")]
[Trait("Component", "Hangfire")]
public class HangfirePostgreSqlTests
{
    private readonly string _connectionString;
    private readonly ITestOutputHelper _output;

    public HangfirePostgreSqlTests(ITestOutputHelper output)
    {
        _output = output;
        
        // Prioridade para a variável de ambiente sugerida pelo usuário para validação no CI
        // Usamos TestConnectionHelper que internamente usa NpgsqlConnectionStringBuilder para maior segurança
        _connectionString = TestConnectionHelper.GetConnectionString();
    }

    [Fact]
    public void UsePostgreSqlStorage_WithValidConnectionString_ShouldInitializeWithoutErrors()
    {
        // This test validates that Hangfire.PostgreSql 1.21.1 is compatible with Npgsql 10.x
        // by attempting to initialize the storage and execute a simple command.
        
        _output.WriteLine($"🔍 Testing Connection String Length: {_connectionString.Length}");
        
        try 
        {
            // Validamos a conexão usando NpgsqlConnectionStringBuilder antes para garantir que está íntegra
            var builder = new NpgsqlConnectionStringBuilder(_connectionString);
            _output.WriteLine($"✅ Connection string parsed successfully. Primary Host: {builder.Host}");

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
            using var connection = storage.GetConnection();
            connection.Should().NotBeNull();
        }
        catch (Exception ex)
        {
            // Logamos detalhes úteis para depuração (mascarando a senha)
            var maskedConn = _connectionString.Contains("Password=") 
                ? _connectionString[..(_connectionString.IndexOf("Password=") + 9)] + "****"
                : "No Password found or hidden";
                
            _output.WriteLine($"❌ FAILED with Connection String: {maskedConn}");
            _output.WriteLine($"❌ Error Message: {ex.Message}");
            throw;
        }
    }
}

using Hangfire.PostgreSql;
using Hangfire.PostgreSql.Factories;
using Npgsql;

namespace MeAjudaAi.Integration.Tests.Infrastructure.Jobs;

[Trait("Category", "Integration")]
[Trait("Component", "Hangfire")]
public class HangfirePostgreSqlTests(ITestOutputHelper output)
{
    private readonly string _connectionString = TestConnectionHelper.GetConnectionString();

    [Fact]
    public void UsePostgreSqlStorage_WithValidConnectionString_ShouldInitializeWithoutErrors()
    {
        // This test validates that Hangfire.PostgreSql 1.21.1 is compatible with Npgsql 10.x
        // by attempting to initialize the storage and execute a simple command.
        
        output.WriteLine($"🔍 Testing Connection String Length: {_connectionString.Length}");
        
        try 
        {
            // Validamos a conexão usando NpgsqlConnectionStringBuilder antes para garantir que está íntegra
            var builder = new NpgsqlConnectionStringBuilder(_connectionString);
            output.WriteLine($"✅ Connection string parsed successfully. Primary Host: {builder.Host}");

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
                
            output.WriteLine($"❌ FAILED with Connection String: {maskedConn}");
            output.WriteLine($"❌ Error Message: {ex.Message}");
            throw;
        }
    }
}

using FluentAssertions;
using MeAjudaAi.ApiService;
using MeAjudaAi.Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Integration.Tests;

/// <summary>
/// 🧪 TESTE DIAGNÓSTICO PARA STARTUP DA APLICAÇÃO
/// 
/// Verifica se há problemas durante a inicialização da aplicação
/// </summary>
public class ApplicationStartupDiagnosticTests(ITestOutputHelper testOutput) : IAsyncLifetime
{
    private SimpleDatabaseFixture? _databaseFixture;

    public async ValueTask InitializeAsync()
    {
        // Define variáveis de ambiente para o processo de teste
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("INTEGRATION_TESTS", "true");

        _databaseFixture = new SimpleDatabaseFixture();
        await _databaseFixture.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_databaseFixture != null)
            await _databaseFixture.DisposeAsync();
    }

    [Fact]
    public async Task Application_Should_Start_Without_Exceptions()
    {
        Exception? startupException = null;
        WebApplicationFactory<Program>? factory = null;

        try
        {
            testOutput.WriteLine("🔧 Creating WebApplicationFactory...");

#pragma warning disable CA2000 // Dispose é gerenciado pelo finally block
            factory = new WebApplicationFactory<Program>()
#pragma warning restore CA2000
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");
                    testOutput.WriteLine("✅ Environment set to Testing");

                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        testOutput.WriteLine("🔧 Configuring test configuration...");

                        // Use the test database connection string
                        if (_databaseFixture?.ConnectionString != null)
                        {
                            testOutput.WriteLine($"🔧 Using test database: {_databaseFixture.ConnectionString}");
                            // Sobrescreve TODAS as configurações de connection strings
                            config.AddInMemoryCollection(new Dictionary<string, string?>
                            {
                                ["ConnectionStrings:DefaultConnection"] = _databaseFixture.ConnectionString,
                                ["ConnectionStrings:UsersDb"] = _databaseFixture.ConnectionString,
                                ["ConnectionStrings:ProvidersDb"] = _databaseFixture.ConnectionString,
                                ["ConnectionStrings:ServiceCatalogsDb"] = _databaseFixture.ConnectionString,
                                ["ConnectionStrings:SearchProvidersDb"] = _databaseFixture.ConnectionString,
                                ["ConnectionStrings:BookingsDb"] = _databaseFixture.ConnectionString,
                                ["Keycloak:Enabled"] = "false",
                                ["Keycloak:ClientSecret"] = "test-secret",
                                ["Keycloak:AdminUsername"] = "test-admin",
                                ["Keycloak:AdminPassword"] = "test-password"
                            });
                        }
                    });

                    builder.ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Warning);
                    });
                });

            testOutput.WriteLine("🚀 Attempting to create client...");

            using var client = factory.CreateClient();
            testOutput.WriteLine("✅ Client created successfully");

            testOutput.WriteLine("🔍 Testing simple request...");

            // Just try to make any request to see if app responds
            var response = await client.GetAsync("/");
            testOutput.WriteLine($"📍 Root endpoint response: {response.StatusCode}");

            // If we get here without exception, startup worked (no explicit assertion needed)
        }
        catch (Exception ex)
        {
            startupException = ex;
            testOutput.WriteLine($"❌ Startup exception: {ex.GetType().Name}");
            testOutput.WriteLine($"❌ Message: {ex.Message}");
            testOutput.WriteLine($"❌ Stack trace: {ex.StackTrace}");

            // Look for inner exceptions
            var innerEx = ex.InnerException;
            while (innerEx != null)
            {
                testOutput.WriteLine($"❌ Inner exception: {innerEx.GetType().Name}");
                testOutput.WriteLine($"❌ Inner message: {innerEx.Message}");
                innerEx = innerEx.InnerException;
            }

            throw; // Re-throw to fail the test
        }
        finally
        {
            if (factory != null)
                await factory.DisposeAsync();
        }
    }
}

using Microsoft.AspNetCore.Mvc.Testing;
using MeAjudaAi.Integration.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using FluentAssertions;

namespace MeAjudaAi.Integration.Tests;

/// <summary>
/// 🧪 TESTE DIAGNÓSTICO PARA STARTUP DA APLICAÇÃO
/// 
/// Verifica se há problemas durante a inicialização da aplicação
/// </summary>
public class ApplicationStartupDiagnosticTest(ITestOutputHelper testOutput) : IAsyncLifetime
{
    private SimpleDatabaseFixture? _databaseFixture;

    public async ValueTask InitializeAsync()
    {
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
            
            factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");
                    testOutput.WriteLine("✅ Environment set to Testing");
                    
                    builder.ConfigureServices(services =>
                    {
                        testOutput.WriteLine("🔧 Configuring test services...");
                        
                        // Add minimal test database context override if needed
                        // (Database fixture should provide connection string)
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

            // If we get here without exception, startup worked
            startupException.Should().BeNull("Application should start without exceptions");
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
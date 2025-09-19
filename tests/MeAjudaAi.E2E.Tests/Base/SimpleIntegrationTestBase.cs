using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Simple integration test base that works without Aspire dependencies
/// Uses TestContainers for PostgreSQL and in-memory configuration
/// </summary>
public abstract class SimpleIntegrationTestBase : IAsyncLifetime
{
    private WebApplicationFactory<Program>? _factory;
    private PostgreSqlContainer? _postgresContainer;
    
    protected HttpClient HttpClient { get; private set; } = null!;
    
    public async Task InitializeAsync()
    {
        // Start PostgreSQL container for testing
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();
            
        await _postgresContainer.StartAsync();
        
        // Create the test application factory
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    // Clear existing configuration sources
                    config.Sources.Clear();
                    
                    // Add minimal test configuration
                    var testConfig = new Dictionary<string, string?>
                    {
                        {"ConnectionStrings:DefaultConnection", _postgresContainer.GetConnectionString()}, // ✅ Nova connection string padrão
                        {"ConnectionStrings:meajudaai-db-local", _postgresContainer.GetConnectionString()},
                        {"ConnectionStrings:users-db", _postgresContainer.GetConnectionString()},
                        {"Postgres:ConnectionString", _postgresContainer.GetConnectionString()},
                        {"ConnectionStrings:Default", _postgresContainer.GetConnectionString()},
                        {"ASPNETCORE_ENVIRONMENT", "Testing"},
                        {"Logging:LogLevel:Default", "Warning"},
                        {"Logging:LogLevel:Microsoft", "Warning"},
                        {"Logging:LogLevel:Microsoft.AspNetCore", "Warning"},
                        // Explicitly disable messaging infrastructure for testing
                        {"Messaging:Enabled", "false"},
                        {"Cache:WarmupEnabled", "false"},
                        // Disable Azure Service Bus and Keycloak for testing
                        {"ServiceBus:Enabled", "false"},
                        {"Keycloak:Enabled", "false"}
                    };
                    
                    config.AddInMemoryCollection(testConfig);
                });
                
                builder.ConfigureServices(services =>
                {
                    // Remove problematic hosted services
                    var hostedServices = services
                        .Where(descriptor => descriptor.ServiceType == typeof(IHostedService))
                        .ToList();
                    
                    foreach (var service in hostedServices)
                    {
                        services.Remove(service);
                    }
                    
                    // Configure minimal logging
                    services.Configure<HostOptions>(options =>
                    {
                        options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
                    });
                });
                
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Warning);
                });
            });
        
        HttpClient = _factory.CreateClient();
        
        // Wait a bit for the application to start
        await Task.Delay(2000);
    }
    
    public async Task DisposeAsync()
    {
        HttpClient?.Dispose();
        _factory?.Dispose();
        
        if (_postgresContainer is not null)
        {
            await _postgresContainer.DisposeAsync();
        }
    }
}
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
/// Classe base unificada para testes de integração E2E
/// Utiliza TestContainers para PostgreSQL e configuração em memória simplificada
/// Substitui OptimizedIntegrationTestBase e SimpleIntegrationTestBase para uniformização
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private WebApplicationFactory<Program>? _factory;
    private PostgreSqlContainer? _postgresContainer;
    
    protected HttpClient HttpClient { get; private set; } = null!;
    
    public async Task InitializeAsync()
    {
        // Inicia container PostgreSQL para testes
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();
            
        await _postgresContainer.StartAsync();
        
        // Cria factory de aplicação de teste com configuração otimizada
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    // Limpa configurações existentes para evitar conflitos
                    config.Sources.Clear();
                    
                    // Adiciona configuração mínima para testes
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
                        {"Logging:LogLevel:Microsoft.EntityFrameworkCore", "Warning"},
                        // Desabilita infraestrutura de messaging para testes
                        {"Messaging:Enabled", "false"},
                        {"Cache:WarmupEnabled", "false"},
                        // Desabilita Azure Service Bus e Keycloak para testes
                        {"ServiceBus:Enabled", "false"},
                        {"Keycloak:Enabled", "false"}
                    };
                    
                    config.AddInMemoryCollection(testConfig);
                });
                
                builder.ConfigureServices(services =>
                {
                    // Remove serviços hospedados problemáticos para evitar conflitos
                    var hostedServices = services
                        .Where(descriptor => descriptor.ServiceType == typeof(IHostedService))
                        .ToList();
                    
                    foreach (var service in hostedServices)
                    {
                        services.Remove(service);
                    }
                    
                    // Configura logging mínimo para evitar problemas com Serilog frozen
                    services.Configure<HostOptions>(options =>
                    {
                        options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
                    });
                });
                
                builder.ConfigureLogging(logging =>
                {
                    // Limpa todos os provedores de logging existentes
                    logging.ClearProviders();
                    // Adiciona apenas console logging com nível Warning
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Warning);
                });
            });
        
        HttpClient = _factory.CreateClient();
        
        // Aguarda um pouco para a aplicação inicializar
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
    
    /// <summary>
    /// Aguarda um serviço ficar disponível com timeout configurável
    /// </summary>
    /// <param name="timeout">Tempo limite para aguardar o serviço</param>
    /// <returns>Task representando a operação assíncrona</returns>
    protected async Task WaitForServiceAsync(TimeSpan timeout)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        while (stopwatch.Elapsed < timeout)
        {
            try
            {
                var response = await HttpClient.GetAsync("/health");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch
            {
                // Ignora exceções durante verificação de saúde
            }
            
            await Task.Delay(1000);
        }
        
        throw new TimeoutException($"Serviço não ficou disponível dentro do tempo limite de {timeout}");
    }
}
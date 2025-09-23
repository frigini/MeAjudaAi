using Aspire.Hosting;

namespace MeAjudaAi.Integration.Tests.Aspire;

/// <summary>
/// Fixture para testes de integração usando Aspire AppHost
/// Configura ambiente completo para testes que envolvem múltiplos módulos
/// </summary>
public class AspireIntegrationFixture : IAsyncLifetime
{
    private DistributedApplication? _app;
    private ResourceNotificationService? _resourceNotificationService;
    
    public HttpClient HttpClient { get; private set; } = null!;
    
    public async Task InitializeAsync()
    {
        // Configura ambiente de teste
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        
        // Cria AppHost para testes
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MeAjudaAi_AppHost>();
        
        _app = await appHost.BuildAsync();
        _resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        
        // Inicia a aplicação
        await _app.StartAsync();
        
        // Aguarda PostgreSQL estar pronto
        await _resourceNotificationService.WaitForResourceAsync("postgres-local", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromMinutes(2));
        
        // Aguarda Redis estar pronto (configurado no AppHost para Testing)
        await _resourceNotificationService.WaitForResourceAsync("redis", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromMinutes(1));
            
        // Aguarda ApiService estar pronto
        await _resourceNotificationService.WaitForResourceAsync("apiservice", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromMinutes(2));
        
        // Configura HttpClient
        HttpClient = _app.CreateHttpClient("apiservice");
    }
    
    public async Task DisposeAsync()
    {
        HttpClient?.Dispose();
        
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}
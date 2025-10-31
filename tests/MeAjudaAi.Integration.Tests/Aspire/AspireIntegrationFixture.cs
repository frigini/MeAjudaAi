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

    public async ValueTask InitializeAsync()
    {
        // Configura ambiente de teste ANTES de criar o AppHost
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("INTEGRATION_TESTS", "true");
        Console.WriteLine($"[AspireIntegrationFixture] ASPNETCORE_ENVIRONMENT = {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
        Console.WriteLine($"[AspireIntegrationFixture] INTEGRATION_TESTS = {Environment.GetEnvironmentVariable("INTEGRATION_TESTS")}");

        // Cria AppHost para testes
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MeAjudaAi_AppHost>();
        Console.WriteLine($"[AspireIntegrationFixture] AppHost Environment = {appHost.Environment?.EnvironmentName}");

        _app = await appHost.BuildAsync();
        _resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        Console.WriteLine("[AspireIntegrationFixture] AppHost built successfully");

        // Inicia a aplicação
        await _app.StartAsync();
        Console.WriteLine("[AspireIntegrationFixture] AppHost started successfully");

        var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

        // Aguarda PostgreSQL estar pronto (skip container wait in CI)
        if (!isCI)
        {
            await _resourceNotificationService.WaitForResourceAsync("postgres-local", KnownResourceStates.Running)
                .WaitAsync(TimeSpan.FromMinutes(3));
        }

        // Aguarda Redis estar pronto (configurado no AppHost para Testing)
        await _resourceNotificationService.WaitForResourceAsync("redis", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromMinutes(2));

        // Aguarda ApiService estar pronto (timeout estendido)
        await _resourceNotificationService.WaitForResourceAsync("apiservice", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromMinutes(4));

        // Configura HttpClient
        HttpClient = _app.CreateHttpClient("apiservice");

        Console.WriteLine("[AspireIntegrationFixture] HttpClient configured - migrations should be handled by application startup");
    }

    public async ValueTask DisposeAsync()
    {
        HttpClient?.Dispose();

        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }
}

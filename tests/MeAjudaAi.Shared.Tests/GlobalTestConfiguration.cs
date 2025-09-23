using MeAjudaAi.Shared.Tests.Infrastructure;

[assembly: CollectionBehavior(DisableTestParallelization = false, MaxParallelThreads = 4)]

namespace MeAjudaAi.Shared.Tests;

/// <summary>
/// Configuração global base para testes de todos os módulos.
/// Define comportamentos comuns de paralelização e configurações de ambiente.
/// </summary>
public static class GlobalTestConfiguration
{
    static GlobalTestConfiguration()
    {
        // Configurar variáveis de ambiente para otimizar testes
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("TEST_SILENT_LOGGING", "true");
        Environment.SetEnvironmentVariable("DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION", "false");
        
        // Configurar cultura invariante para testes consistentes
        Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
    }
}

/// <summary>
/// Fixture base compartilhado para testes de integração que gerencia lifecycle dos containers.
/// Pode ser usado por qualquer módulo que precise de containers compartilhados.
/// </summary>
public class SharedIntegrationTestFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        // Inicia containers compartilhados uma única vez para toda a collection
        await SharedTestContainers.StartAllAsync();
    }

    public async Task DisposeAsync()
    {
        // Para containers quando todos os testes da collection terminarem
        await SharedTestContainers.StopAllAsync();
    }
}
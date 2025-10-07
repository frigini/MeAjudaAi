using MeAjudaAi.Shared.Tests.Auth;
using MeAjudaAi.Shared.Tests.Extensions;
using Xunit.Abstractions;

namespace MeAjudaAi.Shared.Tests.Base;

/// <summary>
/// 🔗 BASE COMPARTILHADA PARA TESTES DE INTEGRAÇÃO ENTRE MÓDULOS
/// 
/// Use esta classe base para testes que precisam de:
/// - RabbitMQ para comunicação entre módulos
/// - Redis para cache distribuído
/// - Ambiente completo de integração
/// - Infraestrutura preparada para múltiplos módulos
/// 
/// Exemplos de uso:
/// - Testes de eventos entre módulos
/// - Fluxos end-to-end completos
/// - Testes de performance com cache
/// 
/// Para testes simples de API, use SharedApiTestBase (mais rápido).
/// </summary>
public abstract class SharedIntegrationTestBase(ITestOutputHelper output) : IAsyncLifetime
{
    protected readonly ITestOutputHelper _output = output;
    protected HttpClient HttpClient { get; set; } = null!;

    public virtual async Task InitializeAsync()
    {
        _output.WriteLine($"🔗 [SharedIntegrationTest] Iniciando teste de integração");

        // HttpClient será configurado pela implementação específica
        // (Aspire, TestContainers, etc.)
        await InitializeInfrastructureAsync();
    }

    /// <summary>
    /// Método abstrato para inicializar a infraestrutura específica
    /// Implementações devem configurar HttpClient e outros serviços
    /// </summary>
    protected abstract Task InitializeInfrastructureAsync();

    public virtual Task DisposeAsync()
    {
        _output.WriteLine($"🧹 [SharedIntegrationTest] Finalizando teste de integração");
        // Não fazemos dispose do HttpClient aqui - ele pode ser compartilhado entre testes
        // O dispose será feito pelo fixture ou pelo factory apropriado
        return Task.CompletedTask;
    }

    /// <summary>
    /// Helper para aguardar processamento assíncrono de mensagens
    /// </summary>
    protected async Task WaitForMessageProcessing(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(5);
        _output.WriteLine($"⏱️ [SharedIntegrationTest] Aguardando processamento de mensagens por {timeout.Value.TotalSeconds}s...");
        await Task.Delay(timeout.Value);
    }

    /// <summary>
    /// Helper para verificar se serviços de integração estão funcionando
    /// </summary>
    protected async Task<bool> VerifyIntegrationServices()
    {
        try
        {
            var healthResponse = await HttpClient.GetAsync("/health");
            var readyResponse = await HttpClient.GetAsync("/health/ready");

            var isHealthy = healthResponse.IsSuccessStatusCode && readyResponse.IsSuccessStatusCode;
            _output.WriteLine($"🏥 [SharedIntegrationTest] Serviços de integração: {(isHealthy ? "✅ Funcionando" : "❌ Com problemas")}");

            return isHealthy;
        }
        catch (Exception ex)
        {
            _output.WriteLine($"❌ [SharedIntegrationTest] Erro ao verificar serviços: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Configura um usuário administrador para o teste (adiciona Authorization header)
    /// </summary>
    public void AuthenticateAsAdmin()
    {
        HttpClient = HttpClient.AsAdmin();
    }

    /// <summary>
    /// Configura um usuário normal para o teste (adiciona Authorization header)
    /// </summary>
    public void AuthenticateAsUser()
    {
        HttpClient = HttpClient.AsUser();
    }

    /// <summary>
    /// Remove a autenticação (usuário anônimo - sem Authorization header)
    /// </summary>
    public void AuthenticateAsAnonymous()
    {
        HttpClient = HttpClient.AsAnonymous();
    }

    /// <summary>
    /// Helper para executar ações em múltiplos módulos
    /// Útil para testes de integração entre módulos
    /// </summary>
    protected async Task ExecuteAcrossModulesAsync(params Func<Task>[] moduleActions)
    {
        foreach (var action in moduleActions)
        {
            await action();
            await WaitForMessageProcessing(TimeSpan.FromSeconds(1)); // Pequena pausa entre módulos
        }
    }

    /// <summary>
    /// Helper para verificar consistência entre módulos
    /// </summary>
    protected async Task<bool> VerifyModuleConsistency(params Func<Task<bool>>[] moduleChecks)
    {
        var results = new List<bool>();

        foreach (var check in moduleChecks)
        {
            var result = await check();
            results.Add(result);
            _output.WriteLine($"📊 [SharedIntegrationTest] Verificação de módulo: {(result ? "✅" : "❌")}");
        }

        var isConsistent = results.All(r => r);
        _output.WriteLine($"🔍 [SharedIntegrationTest] Consistência geral: {(isConsistent ? "✅ OK" : "❌ Problemas detectados")}");

        return isConsistent;
    }
}

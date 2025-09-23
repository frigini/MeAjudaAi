using MeAjudaAi.Integration.Tests.Aspire;
using Xunit;
using Xunit.Abstractions;

namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// 🔗 BASE PARA TESTES DE INTEGRAÇÃO ENTRE MÓDULOS
/// 
/// Use esta classe base para testes que precisam de:
/// - RabbitMQ para comunicação entre módulos
/// - Redis para cache distribuído
/// - Ambiente completo de integração
/// 
/// Exemplos de uso:
/// - Testes de eventos entre módulos
/// - Fluxos end-to-end completos
/// - Testes de performance com cache
/// 
/// Para testes simples de API, use ApiTestBase (mais rápido).
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<AspireIntegrationFixture>, IAsyncLifetime
{
    protected readonly AspireIntegrationFixture _fixture;
    protected readonly ITestOutputHelper _output;
    protected HttpClient HttpClient => _fixture.HttpClient;

    protected IntegrationTestBase(AspireIntegrationFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    public virtual Task InitializeAsync()
    {
        _output.WriteLine($"🔗 [IntegrationTest] Iniciando teste de integração");
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        _output.WriteLine($"🧹 [IntegrationTest] Finalizando teste de integração");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Helper para aguardar processamento assíncrono de mensagens
    /// </summary>
    protected async Task WaitForMessageProcessing(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(5);
        _output.WriteLine($"⏱️ [IntegrationTest] Aguardando processamento de mensagens por {timeout.Value.TotalSeconds}s...");
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
            _output.WriteLine($"🏥 [IntegrationTest] Serviços de integração: {(isHealthy ? "✅ Funcionando" : "❌ Com problemas")}");
            
            return isHealthy;
        }
        catch (Exception ex)
        {
            _output.WriteLine($"❌ [IntegrationTest] Erro ao verificar serviços: {ex.Message}");
            return false;
        }
    }
}
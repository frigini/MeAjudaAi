using MeAjudaAi.Integration.Tests.Aspire;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;

namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// 🔗 BASE PARA TESTES DE INTEGRAÇÃO ENTRE MÓDULOS - ASPIRE
/// 
/// Implementação específica para testes que usam Aspire com:
/// - RabbitMQ para comunicação entre módulos
/// - Redis para cache distribuído
/// - Ambiente completo de integração
/// 
/// Exemplos de uso:
/// - Testes de eventos entre módulos
/// - Fluxos end-to-end completos
/// - Testes de performance com cache
/// 
/// Para testes simples de API, use BaseApiTest (mais rápido).
/// </summary>
public abstract class BaseIntegrationTest(AspireIntegrationFixture fixture, ITestOutputHelper output)
    : BaseSharedIntegrationTest(output), IClassFixture<AspireIntegrationFixture>
{
    private readonly AspireIntegrationFixture _fixture = fixture;

    protected override async Task InitializeInfrastructureAsync()
    {
        // Configura HttpClient a partir do fixture Aspire
        HttpClient = _fixture.HttpClient;
        _output.WriteLine($"🔗 [IntegrationTest] Aspire HttpClient configured");
        await Task.CompletedTask;
    }
}

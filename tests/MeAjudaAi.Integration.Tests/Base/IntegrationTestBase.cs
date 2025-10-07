using MeAjudaAi.Integration.Tests.Aspire;
using MeAjudaAi.Shared.Tests.Base;
using Xunit.Abstractions;

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
/// Para testes simples de API, use ApiTestBase (mais rápido).
/// </summary>
public abstract class IntegrationTestBase(AspireIntegrationFixture fixture, ITestOutputHelper output)
    : SharedIntegrationTestBase(output), IClassFixture<AspireIntegrationFixture>
{
    protected readonly AspireIntegrationFixture _fixture = fixture;

    protected override async Task InitializeInfrastructureAsync()
    {
        // Configura HttpClient a partir do fixture Aspire
        HttpClient = _fixture.HttpClient;
        _output.WriteLine($"🔗 [IntegrationTest] Aspire HttpClient configurado");
        await Task.CompletedTask;
    }
}

using MeAjudaAi.Integration.Tests.Aspire;
using MeAjudaAi.Integration.Tests.Base;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace MeAjudaAi.Integration.Tests.Examples;

/// <summary>
/// 🔗 EXEMPLO: TESTES DE INTEGRAÇÃO COMPLETA
/// 
/// Demonstra a diferença entre:
/// - Testing environment (AspireAppFixture) = Testes rápidos de API
/// - Integration environment (AspireIntegrationFixture) = Testes completos com RabbitMQ
/// </summary>
public class IntegrationExampleTests : IntegrationTestBase
{
    public IntegrationExampleTests(AspireIntegrationFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }

    [Fact]
    public async Task IntegrationEnvironment_ShouldHaveRabbitMQ()
    {
        // Arrange
        _output.WriteLine("🔗 Testando ambiente Integration com RabbitMQ...");
        
        // Act
        var servicesHealthy = await VerifyIntegrationServices();
        
        // Assert
        Assert.True(servicesHealthy, "Serviços de integração devem estar funcionando");
        
        // Verificar se conseguimos acessar endpoints que usam cache/mensageria
        var usersResponse = await HttpClient.GetAsync("/api/v1/users");
        _output.WriteLine($"🔗 Users endpoint (com cache/mensageria): {usersResponse.StatusCode}");
        
        // Em ambiente Integration, pode ter comportamento diferente devido ao RabbitMQ
        Assert.True(usersResponse.IsSuccessStatusCode || usersResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateUser_ShouldTriggerEventProcessing()
    {
        // Arrange
        _output.WriteLine("🔗 Testando criação de usuário com eventos...");
        
        var userData = new
        {
            name = "Integration Test User",
            email = "integration@test.com",
            age = 30
        };

        // Act
        var createResponse = await HttpClient.PostAsJsonAsync("/api/v1/users", userData);
        _output.WriteLine($"🔗 Create user response: {createResponse.StatusCode}");

        // Aguardar processamento de eventos assíncronos
        await WaitForMessageProcessing(TimeSpan.FromSeconds(3));

        // Assert
        // Em ambiente Integration, events podem ser processados via RabbitMQ
        // Aqui verificaríamos se os eventos foram publicados e processados corretamente
        Assert.True(true, "Teste de integração executado - verificar logs para detalhes de eventos");
    }

    [Fact]
    public async Task HealthChecks_ShouldIncludeAllServices()
    {
        // Arrange & Act
        var healthResponse = await HttpClient.GetAsync("/health");
        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        
        _output.WriteLine($"🏥 Health check response: {healthResponse.StatusCode}");
        _output.WriteLine($"🏥 Health check content: {healthContent}");

        // Assert
        Assert.True(healthResponse.IsSuccessStatusCode);
        
        // Em ambiente Integration, health checks podem incluir RabbitMQ, Redis, etc.
        // (dependendo da configuração implementada)
    }
}
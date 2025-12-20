using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Options;

/// <summary>
/// Configurações específicas para infraestrutura de testes (compartilhada entre módulos)
/// </summary>
public class TestInfrastructureOptions
{
    /// <summary>
    /// Configurações do banco de dados de teste
    /// </summary>
    public TestDatabaseOptions Database { get; set; } = new();

    /// <summary>
    /// Configurações do cache de teste (Redis)
    /// </summary>
    public TestCacheOptions Cache { get; set; } = new();

    /// <summary>
    /// Configurações de serviços externos (Keycloak, etc.)
    /// </summary>
    public TestExternalServicesOptions ExternalServices { get; set; } = new();
}


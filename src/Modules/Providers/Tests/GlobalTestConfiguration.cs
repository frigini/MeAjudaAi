using MeAjudaAi.Shared.Tests;

namespace MeAjudaAi.Modules.Providers.Tests;

/// <summary>
/// Collection definition específica para testes de integração do módulo Providers
/// </summary>
[CollectionDefinition("ProvidersIntegrationTests", DisableParallelization = true)]
public class ProvidersIntegrationTestCollection : ICollectionFixture<SharedIntegrationTestFixture>
{
}

using MeAjudaAi.Shared.Tests;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests;

/// <summary>
/// Collection definition específica para testes de integração do módulo ServiceCatalogs
/// DisableParallelization garante que os testes rodem sequencialmente, evitando
/// problemas de duplicate key constraints quando múltiplos testes criam categorias/serviços com mesmo nome
/// </summary>
[CollectionDefinition("ServiceCatalogsIntegrationTests", DisableParallelization = true)]
public class ServiceCatalogsIntegrationTestCollection : ICollectionFixture<SharedIntegrationTestFixture>
{
    // Esta classe não tem implementação - apenas define a collection específica do módulo ServiceCatalogs
}

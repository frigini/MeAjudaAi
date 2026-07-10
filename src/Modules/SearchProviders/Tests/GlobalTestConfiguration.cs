namespace MeAjudaAi.Modules.SearchProviders.Tests;

/// <summary>
/// Collection definition específica para testes de integração do módulo SearchProviders.
/// DisableParallelization impede que classes compartilhando o mesmo DB executem em paralelo.
/// </summary>
[CollectionDefinition("SearchProvidersIntegrationTests", DisableParallelization = true)]
public class SearchProvidersIntegrationTestCollection
{
}

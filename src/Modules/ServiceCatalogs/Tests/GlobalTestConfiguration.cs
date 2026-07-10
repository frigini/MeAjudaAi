namespace MeAjudaAi.Modules.ServiceCatalogs.Tests;

/// <summary>
/// Collection definition específica para testes de integração do módulo ServiceCatalogs.
/// DisableParallelization impede que classes compartilhando o mesmo DB executem em paralelo.
/// </summary>
[CollectionDefinition("ServiceCatalogsIntegrationTests", DisableParallelization = true)]
public class ServiceCatalogsIntegrationTestCollection
{
}

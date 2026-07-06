namespace MeAjudaAi.Modules.Locations.Tests;

/// <summary>
/// Collection definition específica para testes de integração do módulo Locations.
/// DisableParallelization impede que classes compartilhando o mesmo DB executem em paralelo.
/// </summary>
[CollectionDefinition("LocationsIntegrationTests", DisableParallelization = true)]
public class LocationsIntegrationTestCollection
{
}

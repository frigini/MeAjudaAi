namespace MeAjudaAi.Modules.Ratings.Tests;

/// <summary>
/// Collection definition específica para testes de integração do módulo Ratings.
/// DisableParallelization impede que classes compartilhando o mesmo DB executem em paralelo.
/// </summary>
[CollectionDefinition("RatingsIntegrationTests", DisableParallelization = true)]
public class RatingsIntegrationTestCollection
{
}

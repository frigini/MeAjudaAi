namespace MeAjudaAi.Modules.Communications.Tests;

/// <summary>
/// Collection definition específica para testes de integração do módulo Communications.
/// DisableParallelization impede que classes compartilhando o mesmo DB executem em paralelo.
/// </summary>
[CollectionDefinition("CommunicationsIntegrationTests", DisableParallelization = true)]
public class CommunicationsIntegrationTestCollection
{
}

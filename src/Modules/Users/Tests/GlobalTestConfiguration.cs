namespace MeAjudaAi.Modules.Users.Tests;

/// <summary>
/// Collection definition específica para testes de integração do módulo Users.
/// DisableParallelization impede que classes compartilhando o mesmo DB executem em paralelo.
/// </summary>
[CollectionDefinition("UsersIntegrationTests", DisableParallelization = true)]
public class UsersIntegrationTestCollection
{
}

namespace MeAjudaAi.Modules.Bookings.Tests;

/// <summary>
/// Collection definition específica para testes de integração do módulo Bookings.
/// DisableParallelization impede que classes compartilhando o mesmo DB executem em paralelo.
/// </summary>
[CollectionDefinition("BookingsIntegrationTests", DisableParallelization = true)]
public class BookingsIntegrationTestCollection
{
}

using MeAjudaAi.Integration.Tests.Infrastructure;

namespace MeAjudaAi.Integration.Tests.Collections;

/// <summary>
/// Collection definition para compartilhar o database fixture entre todas as classes de teste
/// Isso garante que um único container PostgreSQL seja usado para todos os testes
/// </summary>
[CollectionDefinition("Integration Tests Collection")]
public class IntegrationTestsCollection : ICollectionFixture<SharedDatabaseFixture>
{
    // Esta classe não tem código - serve apenas para definir a coleção
    // O xUnit automaticamente injetará a SharedDatabaseFixture em todas as classes
    // que usarem [Collection("Integration Tests Collection")]
}
using MeAjudaAi.Shared.Tests.TestInfrastructure.Fixtures;

namespace MeAjudaAi.Shared.Tests.Collections;

/// <summary>
/// Collection para testes que podem ser executados em paralelo
/// </summary>
[CollectionDefinition("Parallel")]
public class ParallelTestCollection : ICollectionFixture<SharedTestFixture>
{
    // Esta classe não precisa de implementação
    // Ela apenas define uma collection que usa SharedTestFixture
}

/// <summary>
/// Collection para testes que precisam ser executados sequencialmente
/// (ex: testes que modificam estado global)
/// </summary>
[CollectionDefinition("Sequential", DisableParallelization = true)]
public class SequentialTestCollection
{
    // Esta classe não precisa de implementação
    // Ela define uma collection sequencial
}

/// <summary>
/// Collection para testes de integração que compartilham banco
/// </summary>
[CollectionDefinition("Database", DisableParallelization = true)]
public class DatabaseTestCollection
{
    // Testes de banco devem ser sequenciais para evitar conflitos
}

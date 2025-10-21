using MeAjudaAi.Integration.Tests.Infrastructure;

namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// Classe base para testes de integração com API do módulo Users
/// Herda da nova classe SharedApiTestBase com container PostgreSQL compartilhado
/// </summary>
public abstract class ApiTestBase : SharedApiTestBase<Program>
{
    protected ApiTestBase(SharedDatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    // A nova versão do SharedApiTestBase já lida com:
    // - Container PostgreSQL compartilhado para melhor performance
    // - Connection string mapping automático
    // - Configuração de autenticação teste
    // - Migrations automáticas
    // - Cleanup automático

    // Não precisamos de overrides específicos
}

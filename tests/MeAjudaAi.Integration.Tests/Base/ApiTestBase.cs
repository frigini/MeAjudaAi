using MeAjudaAi.Integration.Tests.Infrastructure;

namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// Classe base para testes de integração com API do módulo Users
/// Herda da nova classe SharedApiTestBase com TestContainers e autenticação configurável
/// </summary>
public abstract class ApiTestBase : SharedApiTestBase<Program>
{
    // A nova versão do SharedApiTestBase já lida com:
    // - TestContainers PostgreSQL
    // - Connection string mapping automático
    // - Configuração de autenticação teste
    // - Migrations automáticas
    // - Cleanup automático

    // Não precisamos de overrides específicos
}

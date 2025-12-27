namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Options;

/// <summary>
/// Configurações do banco de dados de teste
/// </summary>
public class TestDatabaseOptions
{
    /// <summary>
    /// Imagem Docker do PostgreSQL para testes (com suporte PostGIS para dados geográficos)
    /// </summary>
    public string PostgresImage { get; set; } = "postgis/postgis:16-3.4";

    /// <summary>
    /// Nome do banco de dados de teste
    /// </summary>
    public string DatabaseName { get; set; } = "meajudaai_test";

    /// <summary>
    /// Usuário do banco de teste
    /// </summary>
    public string Username { get; set; } = "test_user";

    /// <summary>
    /// Senha do banco de teste
    /// </summary>
    public string Password { get; set; } = "test_password";

    /// <summary>
    /// Schema específico do módulo (ex: users, providers, services)
    /// </summary>
    public string Schema { get; set; } = "users";

    /// <summary>
    /// Se deve aplicar migrations automaticamente
    /// </summary>
    public bool AutoMigrate { get; set; } = true;

    /// <summary>
    /// Se deve usar InMemory database ao invés de PostgreSQL
    /// </summary>
    public bool UseInMemoryDatabase { get; set; } = false;
}

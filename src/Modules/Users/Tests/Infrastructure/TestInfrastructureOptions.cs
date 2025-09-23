namespace MeAjudaAi.Modules.Users.Tests.Infrastructure;

/// <summary>
/// Configurações específicas para infraestrutura de testes do módulo Users
/// </summary>
public class TestInfrastructureOptions
{
    /// <summary>
    /// Configurações do banco de dados de teste
    /// </summary>
    public TestDatabaseOptions Database { get; set; } = new();
    
    /// <summary>
    /// Configurações do cache de teste (Redis)
    /// </summary>
    public TestCacheOptions Cache { get; set; } = new();
    
    /// <summary>
    /// Configurações de serviços externos (Keycloak, etc.)
    /// </summary>
    public TestExternalServicesOptions ExternalServices { get; set; } = new();
}

public class TestDatabaseOptions
{
    /// <summary>
    /// Imagem Docker do PostgreSQL para testes
    /// </summary>
    public string PostgresImage { get; set; } = "postgres:15-alpine";
    
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
    /// Schema específico do módulo
    /// </summary>
    public string Schema { get; set; } = "users";
    
    /// <summary>
    /// Se deve aplicar migrations automaticamente
    /// </summary>
    public bool AutoMigrate { get; set; } = true;
}

public class TestCacheOptions
{
    /// <summary>
    /// Imagem Docker do Redis para testes
    /// </summary>
    public string RedisImage { get; set; } = "redis:7-alpine";
    
    /// <summary>
    /// Se deve usar cache em testes
    /// </summary>
    public bool Enabled { get; set; } = false;
}

public class TestExternalServicesOptions
{
    /// <summary>
    /// Se deve usar mocks para Keycloak
    /// </summary>
    public bool UseKeycloakMock { get; set; } = true;
    
    /// <summary>
    /// Se deve usar mocks para message bus
    /// </summary>
    public bool UseMessageBusMock { get; set; } = true;
}
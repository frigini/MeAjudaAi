namespace MeAjudaAi.Shared.Tests.Infrastructure.Options;

/// <summary>
/// Configurações do cache de teste (Redis)
/// </summary>
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

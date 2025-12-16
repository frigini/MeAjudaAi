namespace MeAjudaAi.AppHost.Options;

/// <summary>
/// Opções de configuração para o setup do PostgreSQL do MeAjudaAi
/// </summary>
public sealed class MeAjudaAiPostgreSqlOptions
{
    /// <summary>
    /// Nome do banco de dados principal da aplicação (agora único para todos os módulos)
    /// </summary>
    public string MainDatabase { get; set; } = "meajudaai";

    /// <summary>
    /// Usuário do PostgreSQL
    /// </summary>
    public string Username { get; set; } = "postgres";

    /// <summary>
    /// Senha do PostgreSQL
    /// </summary>
    public string Password { get; set; } = "";

    /// <summary>
    /// Indica se deve habilitar configuração otimizada para testes
    /// </summary>
    public bool IsTestEnvironment { get; set; }

    /// <summary>
    /// Indica se deve incluir PgAdmin para desenvolvimento
    /// </summary>
    public bool IncludePgAdmin { get; set; } = true;
}

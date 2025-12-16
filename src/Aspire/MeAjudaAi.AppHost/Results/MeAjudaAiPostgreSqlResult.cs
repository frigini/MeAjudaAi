using Aspire.Hosting.ApplicationModel;

namespace MeAjudaAi.AppHost.Results;

/// <summary>
/// Resultado da configuração do PostgreSQL contendo referências ao banco de dados
/// </summary>
public sealed class MeAjudaAiPostgreSqlResult
{
    /// <summary>
    /// Referência ao banco de dados principal da aplicação (único para todos os módulos)
    /// </summary>
    public required IResourceBuilder<IResourceWithConnectionString> MainDatabase { get; init; }
}

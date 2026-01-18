namespace MeAjudaAi.Web.Admin.Services;

/// <summary>
/// Serviço para gerenciar anúncios de região ARIA live para leitores de tela
/// </summary>
public class LiveRegionService
{
    /// <summary>
    /// Evento disparado quando um novo anúncio deve ser feito
    /// </summary>
    public static event Action<string>? OnAnnouncement;

    /// <summary>
    /// Anuncia uma mensagem para leitores de tela
    /// </summary>
    /// <param name="message">A mensagem a ser anunciada</param>
    public void Announce(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            OnAnnouncement?.Invoke(message);
        }
    }

    /// <summary>
    /// Anuncia início de carregamento
    /// </summary>
    public void AnnounceLoadingStarted(string entityName)
    {
        Announce($"Carregando {entityName}...");
    }

    /// <summary>
    /// Anuncia conclusão de carregamento
    /// </summary>
    public void AnnounceLoadingCompleted(string entityName, int count)
    {
        Announce($"{count} {entityName} carregado(s) com sucesso.");
    }

    /// <summary>
    /// Anuncia operação bem-sucedida
    /// </summary>
    public void AnnounceSuccess(string operation, string entityName)
    {
        var message = operation.ToLowerInvariant() switch
        {
            "create" or "criar" => $"{entityName} criado com sucesso.",
            "update" or "atualizar" => $"{entityName} atualizado com sucesso.",
            "delete" or "excluir" => $"{entityName} excluído com sucesso.",
            "verify" or "verificar" => $"{entityName} verificado com sucesso.",
            _ => $"{operation} realizado com sucesso."
        };
        Announce(message);
    }

    /// <summary>
    /// Anuncia erro
    /// </summary>
    public void AnnounceError(string errorMessage)
    {
        Announce($"Erro: {errorMessage}");
    }

    /// <summary>
    /// Anuncia erros de validação
    /// </summary>
    public void AnnounceValidationErrors(int errorCount)
    {
        Announce($"{errorCount} erro(s) de validação encontrado(s).");
    }

    /// <summary>
    /// Anuncia mudança de página
    /// </summary>
    public void AnnouncePageChange(int pageNumber, int totalPages)
    {
        Announce($"Navegado para página {pageNumber} de {totalPages}.");
    }

    /// <summary>
    /// Announce filter applied
    /// </summary>
    public void AnnounceFilterApplied(int resultCount)
    {
        Announce($"Filtro aplicado. {resultCount} resultado(s) encontrado(s).");
    }
}

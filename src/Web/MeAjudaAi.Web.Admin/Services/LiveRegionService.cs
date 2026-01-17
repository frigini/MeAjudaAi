namespace MeAjudaAi.Web.Admin.Services;

/// <summary>
/// Service for managing ARIA live region announcements for screen readers
/// </summary>
public class LiveRegionService
{
    /// <summary>
    /// Event triggered when a new announcement should be made
    /// </summary>
    public static event Action<string>? OnAnnouncement;

    /// <summary>
    /// Announce a message to screen readers
    /// </summary>
    /// <param name="message">The message to announce</param>
    public void Announce(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            OnAnnouncement?.Invoke(message);
        }
    }

    /// <summary>
    /// Announce loading started
    /// </summary>
    public void AnnounceLoadingStarted(string entityName)
    {
        Announce($"Carregando {entityName}...");
    }

    /// <summary>
    /// Announce loading completed
    /// </summary>
    public void AnnounceLoadingCompleted(string entityName, int count)
    {
        Announce($"{count} {entityName} carregado(s) com sucesso.");
    }

    /// <summary>
    /// Announce success operation
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
    /// Announce error
    /// </summary>
    public void AnnounceError(string errorMessage)
    {
        Announce($"Erro: {errorMessage}");
    }

    /// <summary>
    /// Announce validation errors
    /// </summary>
    public void AnnounceValidationErrors(int errorCount)
    {
        Announce($"{errorCount} erro(s) de validação encontrado(s).");
    }

    /// <summary>
    /// Announce page change
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

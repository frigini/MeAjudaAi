namespace MeAjudaAi.Web.Admin.Helpers.Accessibility;

/// <summary>
/// Helper para anúncios de ARIA live regions.
/// Usado para notificar leitores de tela sobre mudanças dinâmicas na página.
/// </summary>
public static class LiveRegionHelper
{
    /// <summary>
    /// Anuncia início de carregamento.
    /// </summary>
    /// <param name="entityName">Nome da entidade sendo carregada</param>
    /// <returns>Mensagem de anúncio</returns>
    public static string LoadingStarted(string entityName) => 
        $"Carregando {entityName}...";
    
    /// <summary>
    /// Anuncia conclusão de carregamento.
    /// </summary>
    /// <param name="entityName">Nome da entidade carregada</param>
    /// <param name="count">Quantidade de itens carregados</param>
    /// <returns>Mensagem de anúncio</returns>
    public static string LoadingCompleted(string entityName, int count) =>
        count == 1
            ? $"{count} {entityName} carregado com sucesso."
            : $"{count} {entityName} carregados com sucesso.";
    
    /// <summary>
    /// Anuncia criação bem-sucedida.
    /// </summary>
    /// <param name="entityName">Nome da entidade criada</param>
    /// <returns>Mensagem de anúncio</returns>
    public static string CreatedSuccess(string entityName) => 
        $"{entityName} criado com sucesso.";
    
    /// <summary>
    /// Anuncia atualização bem-sucedida.
    /// </summary>
    /// <param name="entityName">Nome da entidade atualizada</param>
    /// <returns>Mensagem de anúncio</returns>
    public static string UpdatedSuccess(string entityName) => 
        $"{entityName} atualizado com sucesso.";
    
    /// <summary>
    /// Anuncia exclusão bem-sucedida.
    /// </summary>
    /// <param name="entityName">Nome da entidade excluída</param>
    /// <returns>Mensagem de anúncio</returns>
    public static string DeletedSuccess(string entityName) => 
        $"{entityName} excluído com sucesso.";
    
    /// <summary>
    /// Anuncia ocorrência de erro.
    /// </summary>
    /// <param name="message">Mensagem de erro</param>
    /// <returns>Mensagem de anúncio</returns>
    public static string ErrorOccurred(string message) => 
        $"Erro: {message}";
    
    /// <summary>
    /// Anuncia erros de validação.
    /// </summary>
    /// <param name="errorCount">Quantidade de erros</param>
    /// <returns>Mensagem de anúncio</returns>
    public static string ValidationError(int errorCount) =>
        errorCount == 1
            ? $"{errorCount} erro de validação encontrado."
            : $"{errorCount} erros de validação encontrados.";
    
    /// <summary>
    /// Anuncia mudança de página.
    /// </summary>
    /// <param name="pageNumber">Número da página atual</param>
    /// <param name="totalPages">Total de páginas</param>
    /// <returns>Mensagem de anúncio</returns>
    public static string PageChanged(int pageNumber, int totalPages) => 
        $"Navegado para página {pageNumber} de {totalPages}.";
    
    /// <summary>
    /// Anuncia aplicação de filtro.
    /// </summary>
    /// <param name="resultCount">Quantidade de resultados encontrados</param>
    /// <returns>Mensagem de anúncio</returns>
    public static string FilterApplied(int resultCount) =>
        resultCount == 1
            ? $"Filtro aplicado. {resultCount} resultado encontrado."
            : $"Filtro aplicado. {resultCount} resultados encontrados.";
    
    /// <summary>
    /// Anuncia mudança de seleção.
    /// </summary>
    /// <param name="itemName">Nome do item selecionado</param>
    /// <returns>Mensagem de anúncio</returns>
    public static string SelectionChanged(string itemName) => 
        $"{itemName} selecionado.";
}

using MeAjudaAi.Web.Admin.Helpers.Accessibility;

namespace MeAjudaAi.Web.Admin.Helpers;

/// <summary>
/// Classe helper para recursos de acessibilidade e conformidade com WCAG 2.1 AA.
/// OBSOLETO: Use os helpers específicos em MeAjudaAi.Web.Admin.Helpers.Accessibility
/// (AriaHelper, LiveRegionHelper, KeyboardNavigationHelper, ColorContrastHelper).
/// </summary>
[Obsolete("Use AriaHelper, LiveRegionHelper, KeyboardNavigationHelper ou ColorContrastHelper em vez desta classe. Será removido na v2.0.")]
public static class AccessibilityHelper
{
    /// <summary>
    /// Labels ARIA para ações comuns em português.
    /// </summary>
    [Obsolete("Use AriaHelper.Labels")]
    public static class AriaLabels
    {
        public const string Create = "Criar novo item";
        public const string Edit = "Editar item";
        public const string Delete = "Excluir item";
        public const string View = "Visualizar item";
        public const string Verify = "Verificar item";
        public const string Activate = "Ativar item";
        public const string Deactivate = "Desativar item";
        public const string Upload = "Enviar arquivo";
        public const string Download = "Baixar arquivo";
        public const string Search = "Pesquisar";
        public const string Filter = "Filtrar resultados";
        public const string Sort = "Ordenar";
        public const string NextPage = "Próxima página";
        public const string PreviousPage = "Página anterior";
        public const string FirstPage = "Primeira página";
        public const string LastPage = "Última página";
        public const string CloseDialog = "Fechar diálogo";
        public const string Cancel = "Cancelar";
        public const string Save = "Salvar";
        public const string Submit = "Enviar";
        public const string ToggleMenu = "Alternar menu";
        public const string ToggleDarkMode = "Alternar modo escuro";
        public const string UserMenu = "Menu do usuário";
        public const string Logout = "Sair";
        public const string SkipToContent = "Pular para o conteúdo principal";
    }

    /// <summary>
    /// Anúncios ARIA live region para mudanças de estado.
    /// </summary>
    [Obsolete("Use LiveRegionHelper")]
    public static class LiveRegionAnnouncements
    {
        public static string LoadingStarted(string entityName) => LiveRegionHelper.LoadingStarted(entityName);
        public static string LoadingCompleted(string entityName, int count) => LiveRegionHelper.LoadingCompleted(entityName, count);
        public static string CreatedSuccess(string entityName) => LiveRegionHelper.CreatedSuccess(entityName);
        public static string UpdatedSuccess(string entityName) => LiveRegionHelper.UpdatedSuccess(entityName);
        public static string DeletedSuccess(string entityName) => LiveRegionHelper.DeletedSuccess(entityName);
        public static string ErrorOccurred(string message) => LiveRegionHelper.ErrorOccurred(message);
        public static string ValidationError(int errorCount) => LiveRegionHelper.ValidationError(errorCount);
        public static string PageChanged(int pageNumber, int totalPages) => LiveRegionHelper.PageChanged(pageNumber, totalPages);
        public static string FilterApplied(int resultCount) => LiveRegionHelper.FilterApplied(resultCount);
        public static string SelectionChanged(string itemName) => LiveRegionHelper.SelectionChanged(itemName);
    }

    /// <summary>
    /// Atributos de role para HTML semântico.
    /// </summary>
    [Obsolete("Use AriaHelper.Roles")]
    public static class Roles
    {
        public const string Navigation = "navigation";
        public const string Main = "main";
        public const string Complementary = "complementary";
        public const string Search = "search";
        public const string Alert = "alert";
        public const string Status = "status";
        public const string Dialog = "dialog";
        public const string AlertDialog = "alertdialog";
        public const string Grid = "grid";
        public const string Row = "row";
        public const string GridCell = "gridcell";
    }

    /// <summary>
    /// Documentação de atalhos de teclado.
    /// </summary>
    [Obsolete("Use KeyboardNavigationHelper.Shortcuts")]
    public static class KeyboardShortcuts
    {
        public const string TabDescription = "Tab: Navegar entre elementos";
        public const string ShiftTabDescription = "Shift+Tab: Navegar para trás";
        public const string EnterDescription = "Enter: Ativar elemento ou confirmar";
        public const string EscapeDescription = "Escape: Fechar diálogo ou cancelar";
        public const string SpaceDescription = "Espaço: Ativar checkbox ou toggle";
        public const string ArrowKeysDescription = "Setas: Navegar em listas";
        public const string HomeDescription = "Home: Ir para o início";
        public const string EndDescription = "End: Ir para o final";
    }

    /// <summary>
    /// Obtém label ARIA para ações CRUD comuns.
    /// </summary>
    [Obsolete("Use AriaHelper.GetActionLabel")]
    public static string GetActionLabel(string action, string? itemName = null) =>
        AriaHelper.GetActionLabel(action, itemName);

    /// <summary>
    /// Verifica se o contraste de cores atende aos padrões WCAG AA (4.5:1 para texto normal).
    /// </summary>
    [Obsolete("Use ColorContrastHelper.IsContrastSufficient")]
    public static bool IsContrastSufficient(string backgroundColor, string foregroundColor) =>
        ColorContrastHelper.IsContrastSufficient(backgroundColor, foregroundColor);

    /// <summary>
    /// Obtém ordem de foco recomendada para elementos de diálogo.
    /// </summary>
    [Obsolete("Use KeyboardNavigationHelper.GetFocusOrder")]
    public static int GetFocusOrder(string elementType) =>
        KeyboardNavigationHelper.GetFocusOrder(elementType);

    /// <summary>
    /// Get ARIA description for status values
    /// </summary>
    [Obsolete("Use AriaHelper.GetStatusDescription")]
    public static string GetStatusDescription(string status) =>
        AriaHelper.GetStatusDescription(status);
}

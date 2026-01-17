namespace MeAjudaAi.Web.Admin.Helpers.Accessibility;

/// <summary>
/// Helper para navegação por teclado e atalhos.
/// </summary>
public static class KeyboardNavigationHelper
{
    /// <summary>
    /// Documentação de atalhos de teclado.
    /// </summary>
    public static class Shortcuts
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
    /// Obtém ordem de foco recomendada para elementos de diálogo.
    /// </summary>
    /// <param name="elementType">Tipo do elemento (title, close-button, etc)</param>
    /// <returns>Ordem de foco (0 = sem ordem específica)</returns>
    public static int GetFocusOrder(string elementType)
    {
        return elementType.ToLowerInvariant() switch
        {
            "title" => 1,
            "close-button" => 2,
            "first-input" => 3,
            "other-inputs" => 4,
            "cancel-button" => 5,
            "submit-button" => 6,
            _ => 0
        };
    }
}

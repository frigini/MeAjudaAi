namespace MeAjudaAi.Web.Admin.Helpers.Accessibility;

/// <summary>
/// Helper para labels, roles e descrições ARIA (Accessible Rich Internet Applications).
/// </summary>
public static class AriaHelper
{
    /// <summary>
    /// Labels ARIA para ações comuns em português.
    /// </summary>
    public static class Labels
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
    /// Atributos de role para HTML semântico.
    /// </summary>
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
    /// Obtém label ARIA para ações CRUD comuns.
    /// </summary>
    /// <param name="action">Nome da ação (create, edit, delete, etc)</param>
    /// <param name="itemName">Nome do item (opcional)</param>
    /// <returns>Label ARIA completo</returns>
    public static string GetActionLabel(string action, string? itemName = null)
    {
        var label = action.ToLowerInvariant() switch
        {
            "create" or "add" => Labels.Create,
            "edit" or "update" => Labels.Edit,
            "delete" or "remove" => Labels.Delete,
            "view" or "details" => Labels.View,
            "verify" => Labels.Verify,
            "activate" => Labels.Activate,
            "deactivate" or "disable" => Labels.Deactivate,
            "upload" => Labels.Upload,
            "download" => Labels.Download,
            _ => action
        };

        return itemName != null ? $"{label}: {itemName}" : label;
    }

    /// <summary>
    /// Obtém descrição ARIA para valores de status.
    /// </summary>
    /// <param name="status">Valor do status</param>
    /// <returns>Descrição ARIA do status</returns>
    public static string GetStatusDescription(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "verified" or "verificado" => "Status: Verificado. Provedor aprovado.",
            "pending" or "pendente" => "Status: Pendente. Aguardando verificação.",
            "rejected" or "rejeitado" => "Status: Rejeitado. Provedor não aprovado.",
            "active" or "ativo" or "ativa" => "Status: Ativo. Item habilitado.",
            "inactive" or "inativo" or "inativa" => "Status: Inativo. Item desabilitado.",
            _ => $"Status: {status}"
        };
    }
}

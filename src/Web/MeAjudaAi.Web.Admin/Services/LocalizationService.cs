using System.Globalization;

namespace MeAjudaAi.Web.Admin.Services;

/// <summary>
/// Disposable subscription for OnCultureChanged event.
/// Automatically unsubscribes when disposed to prevent memory leaks.
/// </summary>
public sealed class LocalizationSubscription : IDisposable
{
    private readonly LocalizationService _service;
    private readonly Action _handler;
    private bool _disposed;

    internal LocalizationSubscription(LocalizationService service, Action handler)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _service.OnCultureChanged += _handler;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _service.OnCultureChanged -= _handler;
            _disposed = true;
        }
    }
}

/// <summary>
/// Serviço de localização para gerenciar idiomas da aplicação.
/// Utiliza padrão de subscription com IDisposable para prevenir memory leaks.
/// </summary>
public class LocalizationService
{
    private CultureInfo _currentCulture;
    private readonly Dictionary<string, Dictionary<string, string>> _translations;
    
    internal event Action? OnCultureChanged;

    public LocalizationService()
    {
        _currentCulture = new CultureInfo("pt-BR"); // Default
        _translations = InitializeTranslations();
    }

    /// <summary>
    /// Cultura atual da aplicação.
    /// </summary>
    public CultureInfo CurrentCulture => _currentCulture;

    /// <summary>
    /// Idioma atual (código ISO de 2 letras).
    /// </summary>
    public string CurrentLanguage => _currentCulture.TwoLetterISOLanguageName;

    /// <summary>
    /// Lista de idiomas suportados pela aplicação.
    /// </summary>
    public IReadOnlyList<CultureInfo> SupportedCultures { get; } = new List<CultureInfo>
    {
        new CultureInfo("pt-BR"),
        new CultureInfo("en-US")
    };

    /// <summary>
    /// Altera o idioma da aplicação.
    /// </summary>
    /// <param name="cultureName">Nome da cultura (ex: "pt-BR", "en-US")</param>
    public void SetCulture(string cultureName)
    {
        var culture = SupportedCultures.FirstOrDefault(c => 
            c.Name.Equals(cultureName, StringComparison.OrdinalIgnoreCase));

        if (culture == null)
        {
            throw new ArgumentException($"Culture '{cultureName}' is not supported", nameof(cultureName));
        }

        _currentCulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        OnCultureChanged?.Invoke();
    }

    /// <summary>
    /// Subscribe to culture change events with automatic cleanup via IDisposable.
    /// Use within 'using' statement or dispose manually to prevent memory leaks.
    /// </summary>
    /// <param name="handler">Action to invoke when culture changes</param>
    /// <returns>Disposable subscription that unsubscribes when disposed</returns>
    /// <example>
    /// using var subscription = _localization.Subscribe(StateHasChanged);
    /// // Automatically unsubscribes on disposal
    /// </example>
    public LocalizationSubscription Subscribe(Action handler)
    {
        return new LocalizationSubscription(this, handler);
    }

    /// <summary>
    /// Obtém string localizada pelo nome da chave.
    /// </summary>
    /// <param name="key">Nome da chave (ex: "Common.Save")</param>
    /// <returns>String localizada</returns>
    public string GetString(string key)
    {
        var cultureName = _currentCulture.Name;
        
        if (_translations.TryGetValue(cultureName, out var cultureTranslations) &&
            cultureTranslations.TryGetValue(key, out var translation))
        {
            return translation;
        }

        // Fallback to en-US
        if (_translations.TryGetValue("en-US", out var fallbackTranslations) &&
            fallbackTranslations.TryGetValue(key, out var fallbackTranslation))
        {
            return fallbackTranslation;
        }

        // Return key if not found
        return key;
    }

    /// <summary>
    /// Obtém string localizada formatada com argumentos.
    /// </summary>
    /// <param name="key">Nome da chave</param>
    /// <param name="arguments">Argumentos para formatação</param>
    /// <returns>String localizada e formatada</returns>
    public string GetString(string key, params object[] arguments)
    {
        var format = GetString(key);
        return string.Format(format, arguments);
    }

    /// <summary>
    /// Inicializa dicionários de traduções.
    /// </summary>
    private static Dictionary<string, Dictionary<string, string>> InitializeTranslations()
    {
        return new Dictionary<string, Dictionary<string, string>>
        {
            ["pt-BR"] = new Dictionary<string, string>
            {
                // Common
                ["Common.Loading"] = "Carregando...",
                ["Common.Save"] = "Salvar",
                ["Common.Cancel"] = "Cancelar",
                ["Common.Delete"] = "Excluir",
                ["Common.Edit"] = "Editar",
                ["Common.Search"] = "Pesquisar",
                ["Common.Filter"] = "Filtrar",
                ["Common.Actions"] = "Ações",
                ["Common.Yes"] = "Sim",
                ["Common.No"] = "Não",
                ["Common.Close"] = "Fechar",
                ["Common.Refresh"] = "Atualizar",
                
                // Navigation
                ["Nav.Dashboard"] = "Painel",
                ["Nav.Providers"] = "Provedores",
                ["Nav.Documents"] = "Documentos",
                ["Nav.Profile"] = "Perfil",
                ["Nav.Logout"] = "Sair",
                
                // Providers
                ["Providers.Title"] = "Provedores",
                ["Providers.SearchPlaceholder"] = "Pesquisar por nome, documento ou contato...",
                ["Providers.Name"] = "Nome",
                ["Providers.Document"] = "Documento",
                ["Providers.Contact"] = "Contato",
                ["Providers.Status"] = "Status",
                ["Providers.CreatedAt"] = "Criado em",
                ["Providers.Active"] = "Ativo",
                ["Providers.Inactive"] = "Inativo",
                
                // Validation
                ["Validation.Required"] = "Este campo é obrigatório",
                ["Validation.InvalidEmail"] = "Endereço de e-mail inválido",
                ["Validation.InvalidPhone"] = "Número de telefone inválido",
                ["Validation.InvalidDocument"] = "Número de documento inválido",
                
                // Success
                ["Success.SavedSuccessfully"] = "Salvo com sucesso",
                ["Success.DeletedSuccessfully"] = "Excluído com sucesso",
                ["Success.UpdatedSuccessfully"] = "Atualizado com sucesso",
                
                // Error
                ["Error.GenericError"] = "Ocorreu um erro. Por favor, tente novamente.",
                ["Error.NetworkError"] = "Erro de conexão. Verifique sua internet.",
                ["Error.Unauthorized"] = "Você não tem permissão para esta ação."
            },
            
            ["en-US"] = new Dictionary<string, string>
            {
                // Common
                ["Common.Loading"] = "Loading...",
                ["Common.Save"] = "Save",
                ["Common.Cancel"] = "Cancel",
                ["Common.Delete"] = "Delete",
                ["Common.Edit"] = "Edit",
                ["Common.Search"] = "Search",
                ["Common.Filter"] = "Filter",
                ["Common.Actions"] = "Actions",
                ["Common.Yes"] = "Yes",
                ["Common.No"] = "No",
                ["Common.Close"] = "Close",
                ["Common.Refresh"] = "Refresh",
                
                // Navigation
                ["Nav.Dashboard"] = "Dashboard",
                ["Nav.Providers"] = "Providers",
                ["Nav.Documents"] = "Documents",
                ["Nav.Profile"] = "Profile",
                ["Nav.Logout"] = "Logout",
                
                // Providers
                ["Providers.Title"] = "Providers",
                ["Providers.SearchPlaceholder"] = "Search by name, document, or contact...",
                ["Providers.Name"] = "Name",
                ["Providers.Document"] = "Document",
                ["Providers.Contact"] = "Contact",
                ["Providers.Status"] = "Status",
                ["Providers.CreatedAt"] = "Created At",
                ["Providers.Active"] = "Active",
                ["Providers.Inactive"] = "Inactive",
                
                // Validation
                ["Validation.Required"] = "This field is required",
                ["Validation.InvalidEmail"] = "Invalid email address",
                ["Validation.InvalidPhone"] = "Invalid phone number",
                ["Validation.InvalidDocument"] = "Invalid document number",
                
                // Success
                ["Success.SavedSuccessfully"] = "Saved successfully",
                ["Success.DeletedSuccessfully"] = "Deleted successfully",
                ["Success.UpdatedSuccessfully"] = "Updated successfully",
                
                // Error
                ["Error.GenericError"] = "An error occurred. Please try again.",
                ["Error.NetworkError"] = "Connection error. Check your internet.",
                ["Error.Unauthorized"] = "You don't have permission for this action."
            }
        };
    }
}

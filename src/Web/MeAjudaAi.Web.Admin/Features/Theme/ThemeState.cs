using Fluxor;

namespace MeAjudaAi.Web.Admin.Features.Theme;

/// <summary>
/// Estado Fluxor para gerenciar o tema da aplicação (light/dark mode).
/// </summary>
[FeatureState]
public record ThemeState
{
    /// <summary>
    /// Indica se o dark mode está ativo
    /// </summary>
    public bool IsDarkMode { get; init; }

    /// <summary>
    /// Indica se deve usar preferência do sistema
    /// </summary>
    public bool UseSystemPreference { get; init; } = true;
}

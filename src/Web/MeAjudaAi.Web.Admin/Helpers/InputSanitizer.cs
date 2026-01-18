using Microsoft.AspNetCore.Components;
using MeAjudaAi.Web.Admin.Extensions;

namespace MeAjudaAi.Web.Admin.Helpers;

/// <summary>
/// Helper para sanitizar inputs de formulários e prevenir XSS.
/// </summary>
public static class InputSanitizer
{
    /// <summary>
    /// Sanitiza string para prevenir XSS.
    /// Remove tags HTML, scripts, event handlers e URIs javascript:/data:.
    /// </summary>
    /// <param name="input">String a ser sanitizada</param>
    /// <returns>String sanitizada</returns>
    public static string Sanitize(string? input)
    {
        return ValidationExtensions.SanitizeInput(input);
    }

    /// <summary>
    /// Cria um callback para sanitizar automaticamente ao digitar.
    /// Útil para campos MudTextField onde queremos sanitização em tempo real.
    /// </summary>
    /// <param name="receiver">Objeto receptor do callback (geralmente o componente atual)</param>
    /// <param name="setter">Action que atualiza a propriedade com o valor sanitizado</param>
    /// <returns>EventCallback configurado para sanitização automática</returns>
    public static EventCallback<string> CreateSanitizedCallback(object? receiver, Action<string> setter)
    {
        ArgumentNullException.ThrowIfNull(setter);

        return EventCallback.Factory.Create<string>(receiver ?? new object(), (value) =>
        {
            setter(Sanitize(value));
        });
    }
}

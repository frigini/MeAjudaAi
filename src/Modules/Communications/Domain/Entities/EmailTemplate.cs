using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Communications.Domain.Entities;

/// <summary>
/// Template de e-mail com suporte a override por contexto.
/// </summary>
/// <remarks>
/// Permite que administradores substituam templates padrão por versões customizadas
/// sem alterar o código-fonte. O sistema verifica templates com <see cref="OverrideKey"/>
/// antes de aplicar o template padrão.
/// </remarks>
public sealed class EmailTemplate : BaseEntity
{
    private EmailTemplate() { }

    /// <summary>
    /// Identificador único do template (snake_case). Ex: "user_registered", "provider_approved".
    /// </summary>
    public string TemplateKey { get; private set; } = string.Empty;

    /// <summary>
    /// Chave de override opcional. Permite substituição customizada sem alterar o padrão.
    /// </summary>
    public string? OverrideKey { get; private set; }

    /// <summary>
    /// Assunto do e-mail. Suporta tokens como {{FirstName}}.
    /// </summary>
    public string Subject { get; private set; } = string.Empty;

    /// <summary>
    /// Corpo HTML do e-mail. Suporta tokens como {{FirstName}}.
    /// </summary>
    public string HtmlBody { get; private set; } = string.Empty;

    /// <summary>
    /// Corpo em texto puro (fallback para clientes sem suporte a HTML).
    /// </summary>
    public string TextBody { get; private set; } = string.Empty;

    /// <summary>
    /// Indica se este template está ativo.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Indica se este é um template de sistema (protegido contra deleção).
    /// </summary>
    public bool IsSystemTemplate { get; private set; }

    /// <summary>
    /// Idioma do template. Ex: "pt-BR", "en-US".
    /// </summary>
    public string Language { get; private set; } = "pt-BR";

    /// <summary>
    /// Versão do template (incrementada a cada update).
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// Cria um novo template de e-mail.
    /// </summary>
    public static EmailTemplate Create(
        string templateKey,
        string subject,
        string htmlBody,
        string textBody,
        string language = "pt-BR",
        string? overrideKey = null,
        bool isSystemTemplate = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(htmlBody);
        ArgumentException.ThrowIfNullOrWhiteSpace(textBody);

        return new EmailTemplate
        {
            TemplateKey = templateKey.ToLowerInvariant().Trim(),
            OverrideKey = overrideKey?.ToLowerInvariant().Trim(),
            Subject = subject,
            HtmlBody = htmlBody,
            TextBody = textBody,
            Language = language,
            IsActive = true,
            Version = 1,
            IsSystemTemplate = isSystemTemplate
        };
    }

    /// <summary>
    /// Atualiza o conteúdo do template e incrementa a versão.
    /// </summary>
    public void UpdateContent(string subject, string htmlBody, string textBody)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(htmlBody);
        ArgumentException.ThrowIfNullOrWhiteSpace(textBody);

        Subject = subject;
        HtmlBody = htmlBody;
        TextBody = textBody;
        Version++;
        MarkAsUpdated();
    }

    /// <summary>
    /// Desativa o template.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Ativa o template.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }
}

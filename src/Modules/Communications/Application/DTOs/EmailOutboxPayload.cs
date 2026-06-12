using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Communications.Application.DTOs;

/// <summary>
/// Payload para mensagens de e-mail no outbox.
/// </summary>
/// <remarks>
/// <para>
/// O payload suporta dois modos de construção, mutuamente exclusivos:
/// </para>
/// <list type="number">
///   <item><description>
///     <strong>Template mode</strong>: defina <see cref="TemplateKey"/> e opcionalmente
///     <see cref="TemplateData"/>. O template será resolvido pelo provider de e-mail.
///   </description></item>
///   <item><description>
///     <strong>Direct body mode</strong>: defina <see cref="HtmlBody"/> (HTML) ou
///     <see cref="TextBody"/> (texto puro). <c>HtmlBody</c> tem precedência sobre
///     <c>TextBody</c> quando ambos são fornecidos.
///   </description></item>
/// </list>
/// <para>
/// NÃO defina mais de um modo. A construção validará e lançará <see cref="ArgumentException"/>
/// se houver conflito (ex: <c>TemplateKey</c> + <c>HtmlBody</c>, ou <c>HtmlBody</c> + <c>TextBody</c>).
/// </para>
/// </remarks>
public sealed record EmailOutboxPayload
{
    /// <summary>Endereço de e-mail do destinatário.</summary>
    public string To { get; init; } = string.Empty;

    /// <summary>Assunto do e-mail.</summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>Corpo em formato HTML. Usado no modo direto. Tem precedência sobre <c>TextBody</c>.</summary>
    public string? HtmlBody { get; init; }

    /// <summary>Corpo em texto puro. Usado no modo direto quando <c>HtmlBody</c> não é fornecido.</summary>
    public string? TextBody { get; init; }

    /// <summary>Endereço de e-mail do remetente (opcional). Se nulo, usa o remetente padrão do provider.</summary>
    public string? From { get; init; }

    /// <summary>Chave do template de e-mail a ser resolvido pelo provider.</summary>
    public string? TemplateKey { get; init; }

    /// <summary>Dados para preenchimento do template (opcional). Deve ser um dicionário somente leitura.</summary>
    public IReadOnlyDictionary<string, string>? TemplateData { get; init; }

    /// <summary>
    /// Cria um novo <see cref="EmailOutboxPayload"/> com validação de modos mutuamente exclusivos.
    /// </summary>
    /// <exception cref="ArgumentException">Se mais de um modo for especificado.</exception>
    public static EmailOutboxPayload Create(
        string to,
        string subject,
        string? htmlBody = null,
        string? textBody = null,
        string? from = null,
        string? templateKey = null,
        IReadOnlyDictionary<string, string>? templateData = null)
    {
        var hasTemplate = !string.IsNullOrEmpty(templateKey);
        var hasDirectBody = !string.IsNullOrEmpty(htmlBody) || !string.IsNullOrEmpty(textBody);

        if (hasTemplate && hasDirectBody)
            throw new ArgumentException("TemplateKey/TemplateData não pode ser combinado com HtmlBody ou TextBody.", nameof(templateKey));

        if (!string.IsNullOrEmpty(htmlBody) && !string.IsNullOrEmpty(textBody))
            throw new ArgumentException("HtmlBody e TextBody não podem ser definidos simultaneamente. Prefira HtmlBody para HTML ou TextBody para texto puro.", nameof(htmlBody));

        return new EmailOutboxPayload
        {
            To = to,
            Subject = subject,
            HtmlBody = htmlBody,
            TextBody = textBody,
            From = from,
            TemplateKey = templateKey,
            TemplateData = templateData
        };
    }
}

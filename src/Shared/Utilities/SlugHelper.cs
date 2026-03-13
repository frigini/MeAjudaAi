using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MeAjudaAi.Shared.Utilities;

/// <summary>
/// Helper para geração de slugs amigáveis para URL
/// </summary>
public static partial class SlugHelper
{
    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleWhitespaceRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultipleHifenRegex();

    /// <summary>
    /// Gera um slug a partir de um texto
    /// </summary>
    /// <param name="text">Texto original</param>
    /// <returns>Slug formatado</returns>
    public static string Generate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // 1. Lowercase e Normalização
        string slug = text.ToLowerInvariant();
        slug = RemoveDiacritics(slug);

        // 2. Remover caracteres não alfanuméricos (exceto espaços e hifens)
        slug = NonAlphanumericRegex().Replace(slug, "");

        // 3. Substituir espaços por hifens
        slug = MultipleWhitespaceRegex().Replace(slug, "-");

        // 4. Remover hifens duplicados
        slug = MultipleHifenRegex().Replace(slug, "-");

        // 5. Trim hifens das extremidades
        return slug.Trim('-');
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}

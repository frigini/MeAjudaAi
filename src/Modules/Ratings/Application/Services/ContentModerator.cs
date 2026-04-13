using System.Text.RegularExpressions;

namespace MeAjudaAi.Modules.Ratings.Application.Services;

public interface IContentModerator
{
    bool IsClean(string? content);
}

public class ContentModerator : IContentModerator
{
    private static readonly string[] BadWords = ["idiota", "burro", "lixo", "golpe", "fake"]; // Exemplo simplificado

    public bool IsClean(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return true;

        var lowerContent = content.ToLower();
        return !BadWords.Any(word => lowerContent.Contains(word));
    }
}

using System.Text.RegularExpressions;

namespace MeAjudaAi.Modules.Ratings.Application.Services;

public interface IContentModerator
{
    bool IsClean(string? content);
}

public class ContentModerator : IContentModerator
{
    private static readonly string[] BadWords = ["idiota", "burro", "lixo", "golpe", "fake"];
    private static readonly List<Regex> Patterns = BadWords
        .Select(word => new Regex($@"\b{Regex.Escape(word)}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled))
        .ToList();

    public bool IsClean(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return true;

        return !Patterns.Any(regex => regex.IsMatch(content));
    }
}

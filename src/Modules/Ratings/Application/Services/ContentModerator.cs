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

        var diagPath = @"C:\Code\MeAjudaAi\tests\MeAjudaAi.E2E.Tests\bin\Debug\net10.0\db_diag.log";
        System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] [MODERATOR] IsClean starting...{System.Environment.NewLine}");
        var result = !Patterns.Any(regex => regex.IsMatch(content));
        System.IO.File.AppendAllText(diagPath, $"[{System.DateTime.UtcNow:O}] [MODERATOR] IsClean completed. Result: {result}{System.Environment.NewLine}");
        return result;
    }
}

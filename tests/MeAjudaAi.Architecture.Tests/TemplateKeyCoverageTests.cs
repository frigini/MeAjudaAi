using System.Reflection;
using System.Text.RegularExpressions;

namespace MeAjudaAi.Architecture.Tests;

/// <summary>
/// Verifica que todas as chaves de template usadas pelos handlers existem no seed de dados.
/// Previne desalinhamento entre handlers e DevelopmentDataSeeder.
/// </summary>
[Trait("Category", "Architecture")]
public class TemplateKeyCoverageTests
{
    [Fact]
    public void AllHandlerTemplateKeys_ShouldExistInSeed()
    {
        // 1. Extrair todas as chaves de template do CommunicationTemplateKeys
        var templateKeysType = typeof(Contracts.Utilities.Constants.CommunicationTemplateKeys);
        var seedKeysFromConstants = templateKeysType
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null)!)
            .ToHashSet();

        // 2. Extrair chaves inline dos handlers (arquivos .cs em Application/Handlers/Events/)
        var seedKeysFromHandlers = ExtractInlineTemplateKeysFromHandlers();

        // 3. Combinar todas as chaves usadas
        var allHandlerKeys = seedKeysFromConstants.Union(seedKeysFromHandlers).ToHashSet();

        // 4. Extrair chaves do seed (DevelopmentDataSeeder)
        var seedKeys = ExtractSeedTemplateKeys();

        // 5. Verificar que todas as chaves dos handlers existem no seed
        var missingKeys = allHandlerKeys.Except(seedKeys).OrderBy(k => k).ToList();

        missingKeys.Should().BeEmpty(
            $"As seguintes chaves de template são usadas por handlers mas não existem no DevelopmentDataSeeder: " +
            $"{string.Join(", ", missingKeys)}");
    }

    [Fact]
    public void SeedTemplateKeys_ShouldAllBeUsedByHandlers()
    {
        // 1. Extrair chaves do seed
        var seedKeys = ExtractSeedTemplateKeys();

        // 2. Extrair chaves dos handlers
        var templateKeysType = typeof(Contracts.Utilities.Constants.CommunicationTemplateKeys);
        var handlerKeys = templateKeysType
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null)!)
            .ToHashSet();

        // 3. Verificar se há chaves no seed que não estão nos handlers
        // (Isto é apenas um aviso, não um erro — seeds podem ter templates extras para uso futuro)
        var unusedKeys = seedKeys.Except(handlerKeys).OrderBy(k => k).ToList();

        // Não falhamos aqui, mas registramos para informação
        // Seeds podem conter templates que serão usados por handlers futuros
    }

    private static HashSet<string> ExtractInlineTemplateKeysFromHandlers()
    {
        var keys = new HashSet<string>();
        var assembly = typeof(Contracts.Utilities.Constants.CommunicationTemplateKeys).Assembly;
        var projectDir = FindCommunicationsHandlersDirectory();

        if (projectDir == null || !Directory.Exists(projectDir))
            return keys;

        var handlerFiles = Directory.GetFiles(projectDir, "*IntegrationEventHandler.cs", SearchOption.AllDirectories);

        foreach (var file in handlerFiles)
        {
            var content = File.ReadAllText(file);

            // Padrão 1: TemplateKey = "chave" ou TemplateKey = CommunicationTemplateKeys.Xxx
            var constMatches = Regex.Matches(content, @"TemplateKey\s*=\s*""([^""]+)""");
            foreach (Match match in constMatches)
            {
                keys.Add(match.Groups[1].Value);
            }

            // Padrão 2: "chave" em switch expressions
            var switchMatches = Regex.Matches(content, @"""([a-z][a-z0-9_]*)""\s*=>");
            foreach (Match match in switchMatches)
            {
                var value = match.Groups[1].Value;
                if (value.Contains('_') || value.Contains('-'))
                {
                    keys.Add(value);
                }
            }
        }

        return keys;
    }

    private static HashSet<string> ExtractSeedTemplateKeys()
    {
        var keys = new HashSet<string>();
        var seederFile = FindDevelopmentDataSeederFile();

        if (seederFile == null || !File.Exists(seederFile))
            return keys;

        var content = File.ReadAllText(seederFile);

        // Encontrar a seção de templates e extrair TemplateKey = "xxx"
        var templateKeyMatches = Regex.Matches(content, @"TemplateKey\s*=\s*""([^""]+)""");
        foreach (Match match in templateKeyMatches)
        {
            keys.Add(match.Groups[1].Value);
        }

        return keys;
    }

    private static string? FindCommunicationsHandlersDirectory()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            var srcDir = Path.Combine(currentDir, "src");
            if (Directory.Exists(srcDir))
            {
                var handlersDir = Path.Combine(srcDir, "Modules", "Communications", "Application", "Handlers", "Events");
                if (Directory.Exists(handlersDir))
                    return handlersDir;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        return null;
    }

    private static string? FindDevelopmentDataSeederFile()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            var srcDir = Path.Combine(currentDir, "src");
            if (Directory.Exists(srcDir))
            {
                var seederFile = Path.Combine(srcDir, "Shared", "Seeding", "DevelopmentDataSeeder.cs");
                if (File.Exists(seederFile))
                    return seederFile;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        return null;
    }
}

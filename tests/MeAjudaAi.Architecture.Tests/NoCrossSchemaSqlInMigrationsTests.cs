using System.Text.RegularExpressions;

namespace MeAjudaAi.Architecture.Tests;

public sealed class NoCrossSchemaSqlInMigrationsTests
{
    // Ajuste se surgir novo módulo/schema
    private static readonly string[] Schemas = new[]
    {
        "bookings",
        "communications",
        "documents",
        "locations",
        "payments",
        "providers",
        "ratings",
        "search_providers",
        "service_catalogs",
        "users"
    };

    // Padrões proibidos em migrations
    private static readonly Regex Forbidden =
        new(@"\b(FROM|JOIN|INSERT\s+INTO|UPDATE\b.*\bFROM|DELETE\s+FROM|REFERENCES)\s+(?<schema>[a-z_]+)\.",
                  RegexOptions.IgnoreCase | RegexOptions.Compiled);

    [Fact]
    public void Migrations_Should_Not_Reference_Other_Module_Schemas()
    {
        // Navega para a raiz do projeto a partir de tests/MeAjudaAi.Architecture.Tests/bin/Debug/...
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (currentDir != null && !File.Exists(Path.Combine(currentDir.FullName, "MeAjudaAi.slnx")))
        {
            currentDir = currentDir.Parent;
        }

        if (currentDir == null)
        {
            throw new Exception("Não foi possível encontrar a raiz do projeto (MeAjudaAi.slnx)");
        }

        var root = Path.Combine(currentDir.FullName, "src");
        var files = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => p.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}",
                                   StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var offenders = new List<string>();

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            foreach (Match m in Forbidden.Matches(text))
            {
                var schema = m.Groups["schema"].Value.ToLowerInvariant();
                // Permite o próprio schema do módulo (ex.: providers.* dentro do Providers)
                if (!Schemas.Contains(schema)) continue;
                if (IsOwnSchema(file, schema)) continue;

                offenders.Add($"{file}:{m.Index} -> {m.Value}");
            }
        }

        offenders.Should().BeEmpty("migrations não devem referenciar tabelas de outros módulos (acoplamento físico). Encontrados:\n" + string.Join("\n", offenders));
    }

    private static bool IsOwnSchema(string migrationFile, string schema)
    {
        // heurística simples: deduz o módulo pelo caminho
        // src/Modules/<Modulo>/.../Migrations/*.cs => próprio schema se o diretório batter
        var parts = migrationFile.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        var i = Array.FindIndex(parts, p => p.Equals("Modules", StringComparison.OrdinalIgnoreCase));
        if (i >= 0 && i + 1 < parts.Length)
        {
            var module = parts[i + 1].ToLowerInvariant();

            // Mapeamento de Nome do Diretório -> Nome do Schema
            var moduleSchema = module switch
            {
                "bookings" => "bookings",
                "communications" => "communications",
                "documents" => "documents",
                "locations" => "locations",
                "payments" => "payments",
                "providers" => "providers",
                "ratings" => "ratings",
                "searchproviders" => "search_providers",
                "servicecatalogs" => "service_catalogs",
                "users" => "users",
                _ => module
            };
            return moduleSchema == schema;
        }
        return false;
    }
}
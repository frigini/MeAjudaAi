using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Database;

/// <summary>
/// Constantes para nomes dos scripts SQL de schema isolation.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class SqlScripts
{
    private const string BasePath = "infrastructure/database/modules";

    public const string Roles = "00-roles.sql";
    public const string Permissions = "01-permissions.sql";

    public static string GetScriptPath(string moduleName, string scriptName)
        => Path.Combine(BasePath, moduleName, scriptName);
}

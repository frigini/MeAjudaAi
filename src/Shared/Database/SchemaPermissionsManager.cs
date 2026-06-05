using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text.RegularExpressions;

namespace MeAjudaAi.Shared.Database;

/// <summary>
/// Gerencia permissões de schema usando scripts SQL da infraestrutura.
/// Implementação genérica para suportar múltiplos módulos.
/// </summary>
public class SchemaPermissionsManager(ILogger<SchemaPermissionsManager> logger)
{
    private const string BasePath = "infrastructure/database/modules";

    private static readonly Regex IdentifierRegex = new(@"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);

    /// <summary>
    /// Configura permissões usando os scripts padronizados (00-roles.sql, 01-permissions.sql).
    /// </summary>
    public async Task EnsureModulePermissionsAsync(string adminConnectionString, ModulePermissionConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.RolePassword) || string.IsNullOrWhiteSpace(config.AppRolePassword))
            throw new ArgumentException("Passwords cannot be null or whitespace.");

        logger.LogInformation("Configurando permissões para o módulo {ModuleName}", config.ModuleName);

        using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();

        try
        {
            await ExecuteScriptAsync(connection, config, "00-roles.sql");
            await ExecuteScriptAsync(connection, config, "01-permissions.sql");

            logger.LogInformation("✅ Permissões configuradas com sucesso para {ModuleName}", config.ModuleName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Erro configurando permissões para {ModuleName}", config.ModuleName);
            throw;
        }
    }

    private static string QuoteIdentifier(string identifier) =>
        "\"" + identifier.Replace("\"", "\"\"") + "\"";

    private static void EnsureValidIdentifier(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value) || !IdentifierRegex.IsMatch(value))
            throw new ArgumentException($"Invalid identifier for {paramName}. Only ASCII letters, digits and underscores are allowed, and it must not start with a digit.", paramName);
    }

    private async Task ExecuteScriptAsync(NpgsqlConnection connection, ModulePermissionConfig config, string scriptName)
    {
        EnsureValidIdentifier(config.RoleName, nameof(config.RoleName));
        EnsureValidIdentifier(config.AppRoleName, nameof(config.AppRoleName));
        EnsureValidIdentifier(config.SchemaName, nameof(config.SchemaName));

        var scriptPath = Path.Combine(BasePath, config.ModuleName, scriptName);
        if (!File.Exists(scriptPath))
            throw new FileNotFoundException($"Script não encontrado: {scriptPath}");

        var sql = await File.ReadAllTextAsync(scriptPath);

        // Replace identifier placeholders only after validation and quoting.
        sql = sql.Replace("{{ROLE_NAME}}", QuoteIdentifier(config.RoleName))
                 .Replace("{{APP_ROLE_NAME}}", QuoteIdentifier(config.AppRoleName))
                 .Replace("{{SCHEMA_NAME}}", QuoteIdentifier(config.SchemaName))
                 // For passwords (literals) use parameters instead of inlining
                 .Replace("{{ROLE_PASSWORD}}", "@role_pwd")
                 .Replace("{{APP_ROLE_PASSWORD}}", "@app_pwd");

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        // Add password parameters (safe as bind parameters)
        command.Parameters.AddWithValue("role_pwd", config.RolePassword);
        command.Parameters.AddWithValue("app_pwd", config.AppRolePassword);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Verifica se as permissões do módulo já estão configuradas.
    /// </summary>
    public async Task<bool> AreModulePermissionsConfiguredAsync(string adminConnectionString, string schemaName, string roleName)
    {
        try
        {
            using var connection = new NpgsqlConnection(adminConnectionString);
            await connection.OpenAsync();

            var sql = """
                SELECT EXISTS (
                    SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = @RoleName
                ) AND EXISTS (
                    SELECT 1 FROM information_schema.schemata WHERE schema_name = @SchemaName
                );
                """;

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("RoleName", roleName);
            command.Parameters.AddWithValue("SchemaName", schemaName);
            
            var result = await command.ExecuteScalarAsync();
            return (bool)(result ?? false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Erro ao verificar permissões do módulo {SchemaName}, assumindo não configurado", schemaName);
            return false;
        }
    }
}

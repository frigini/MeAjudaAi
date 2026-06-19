using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using MeAjudaAi.Shared.Authorization.Core.Models;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MeAjudaAi.Shared.Database;

/// <summary>
/// Gerencia permissões de schema usando scripts SQL da infraestrutura.
/// Implementação genérica para suportar múltiplos módulos.
/// </summary>
public class SchemaPermissionsManager(ILogger<SchemaPermissionsManager> logger)
{
    private static readonly Regex IdentifierRegex = new(@"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);

    /// <summary>
    /// Configura permissões usando scripts SQL padronizados (00-roles.sql, 01-permissions.sql).
    /// </summary>
    public virtual async Task EnsureModulePermissionsAsync(string adminConnectionString, ModulePermissionConfig config)
    {
        // 1. Validar antes de abrir conexão
        if (string.IsNullOrWhiteSpace(config.RolePassword) || string.IsNullOrWhiteSpace(config.AppRolePassword))
            throw new ArgumentException("Passwords cannot be null or whitespace.");
        
        EnsureValidIdentifier(config.RoleName, nameof(config.RoleName));
        EnsureValidIdentifier(config.AppRoleName, nameof(config.AppRoleName));
        EnsureValidIdentifier(config.SchemaName, nameof(config.SchemaName));

        var rolesScriptPath = SqlScripts.GetScriptPath(config.ModuleName, SqlScripts.Roles);
        var permsScriptPath = SqlScripts.GetScriptPath(config.ModuleName, SqlScripts.Permissions);
        
        if (!File.Exists(rolesScriptPath)) throw new FileNotFoundException($"Script not found: {rolesScriptPath}");
        if (!File.Exists(permsScriptPath)) throw new FileNotFoundException($"Script not found: {permsScriptPath}");

        logger.LogInformation("Configuring permissions for module {ModuleName}", config.ModuleName);

        using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();

        try
        {
            await ExecuteScriptAsync(connection, config, SqlScripts.Roles);
            await ExecuteScriptAsync(connection, config, SqlScripts.Permissions);

            logger.LogInformation("Successfully configured permissions for {ModuleName}", config.ModuleName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error configuring permissions for {ModuleName}", config.ModuleName);
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

    [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "SQL template is a static file from disk; replacements use validated identifiers via EnsureValidIdentifier.")]
    private async Task ExecuteScriptAsync(NpgsqlConnection connection, ModulePermissionConfig config, string scriptName)
    {
        var scriptPath = SqlScripts.GetScriptPath(config.ModuleName, scriptName);
        var sql = await File.ReadAllTextAsync(scriptPath);

        // Replace placeholders safely
        sql = sql.Replace(SqlTemplatePlaceholders.RoleName, QuoteIdentifier(config.RoleName))
                 .Replace(SqlTemplatePlaceholders.SchemaNameLiteral, $"'{config.SchemaName.Replace("'", "''")}'")
                 .Replace(SqlTemplatePlaceholders.RoleOwnerName, QuoteIdentifier(config.RoleName + "_owner"))
                 .Replace(SqlTemplatePlaceholders.AppRoleName, QuoteIdentifier(config.AppRoleName))
                 .Replace(SqlTemplatePlaceholders.RolePassword, $"@{SqlTemplatePlaceholders.RolePwdParam}")
                 .Replace(SqlTemplatePlaceholders.AppRolePassword, $"@{SqlTemplatePlaceholders.AppPwdParam}")
                 .Replace(SqlTemplatePlaceholders.SchemaName, QuoteIdentifier(config.SchemaName));

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.AddWithValue(SqlTemplatePlaceholders.RolePwdParam, config.RolePassword);
        command.Parameters.AddWithValue(SqlTemplatePlaceholders.AppPwdParam, config.AppRolePassword);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Verifica se as permissões do módulo já estão configuradas.
    /// </summary>
    public virtual async Task<bool> AreModulePermissionsConfiguredAsync(string adminConnectionString, string schemaName, string roleName)
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

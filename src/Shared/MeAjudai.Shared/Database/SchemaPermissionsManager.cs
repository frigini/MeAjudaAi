using Microsoft.Extensions.Logging;
using Npgsql;

namespace MeAjudaAi.Shared.Database;

/// <summary>
/// Gerencia permissões de schema usando scripts SQL existentes da infraestrutura.
/// Executa apenas quando necessário e de forma modular.
/// </summary>
public class SchemaPermissionsManager(ILogger<SchemaPermissionsManager> logger)
{
    /// <summary>
    /// Configura permissões usando os scripts existentes em infrastructure/database/schemas
    /// </summary>
    public async Task EnsureUsersModulePermissionsAsync(
        string adminConnectionString,
        string usersRolePassword = "users_secret",
        string appRolePassword = "app_secret")
    {
        logger.LogInformation("Configurando permissões para módulo Users usando scripts existentes");

        using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();

        try
        {
            // Executar os scripts na ordem correta
            // NOTA: Schema 'users' será criado automaticamente pelo EF Core durante as migrações
            await ExecuteSchemaScript(connection, "00-roles", usersRolePassword, appRolePassword);
            await ExecuteSchemaScript(connection, "01-permissions");

            logger.LogInformation("✅ Permissões configuradas com sucesso para módulo Users");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Erro ao configurar permissões para módulo Users");
            throw;
        }
    }

    /// <summary>
    /// Cria connection string para o usuário específico do módulo Users
    /// </summary>
    public string CreateUsersModuleConnectionString(
        string baseConnectionString,
        string usersRolePassword = "users_secret")
    {
        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString);
        builder.Username = "users_role";
        builder.Password = usersRolePassword;
        builder.SearchPath = "users,public"; // Schema users primeiro, public como fallback

        return builder.ToString();
    }

    /// <summary>
    /// Verifica se as permissões do módulo Users já estão configuradas
    /// </summary>
    public async Task<bool> AreUsersPermissionsConfiguredAsync(string adminConnectionString)
    {
        try
        {
            using var connection = new NpgsqlConnection(adminConnectionString);
            await connection.OpenAsync();

            var result = await ExecuteScalarAsync<bool>(connection, """
                SELECT EXISTS (
                    SELECT 1 FROM pg_catalog.pg_roles 
                    WHERE rolname = 'users_role'
                ) AND EXISTS (
                    SELECT 1 FROM information_schema.schemata 
                    WHERE schema_name = 'users'
                );
                """);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Erro ao verificar permissões do módulo Users, assumindo não configurado");
            return false;
        }
    }

    private async Task ExecuteSchemaScript(NpgsqlConnection connection, string scriptType, params string[] parameters)
    {
        string sql = scriptType switch
        {
            "00-roles" => GetCreateRolesScript(parameters[0], parameters[1]),
            "01-permissions" => GetGrantPermissionsScript(),
            _ => throw new ArgumentException($"Script type '{scriptType}' not recognized")
        };

        logger.LogDebug("Executando script: {ScriptType}", scriptType);
        await ExecuteSqlAsync(connection, sql);
    }

    private string GetCreateRolesScript(string usersPassword, string appPassword) => $"""
        -- Create dedicated role for users module
        DO $$
        BEGIN
            IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'users_role') THEN
                CREATE ROLE users_role LOGIN PASSWORD '{usersPassword}';
            END IF;
        END
        $$;

        -- Create a general application role for cross-cutting operations
        DO $$
        BEGIN
            IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'meajudaai_app_role') THEN
                CREATE ROLE meajudaai_app_role LOGIN PASSWORD '{appPassword}';
            END IF;
        END
        $$;

        -- Grant necessary permissions to app role to manage users
        GRANT users_role TO meajudaai_app_role;
        """;

    private string GetGrantPermissionsScript() => """
        -- Grant permissions for users module
        GRANT USAGE ON SCHEMA users TO users_role;
        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA users TO users_role;
        GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA users TO users_role;

        -- Set default privileges for future tables and sequences in users schema
        ALTER DEFAULT PRIVILEGES IN SCHEMA users GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO users_role;
        ALTER DEFAULT PRIVILEGES IN SCHEMA users GRANT USAGE, SELECT ON SEQUENCES TO users_role;

        -- Set default search path for users_role
        ALTER ROLE users_role SET search_path = users, public;

        -- Grant cross-schema permissions to app role
        GRANT USAGE ON SCHEMA users TO meajudaai_app_role;
        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA users TO meajudaai_app_role;
        GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA users TO meajudaai_app_role;

        -- Set default privileges for app role
        ALTER DEFAULT PRIVILEGES IN SCHEMA users GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO meajudaai_app_role;
        ALTER DEFAULT PRIVILEGES IN SCHEMA users GRANT USAGE, SELECT ON SEQUENCES TO meajudaai_app_role;

        -- Set search path for app role to include all necessary schemas
        ALTER ROLE meajudaai_app_role SET search_path = users, public;

        -- Grant permissions on public schema
        GRANT USAGE ON SCHEMA public TO users_role;
        GRANT USAGE ON SCHEMA public TO meajudaai_app_role;
        GRANT CREATE ON SCHEMA public TO meajudaai_app_role;
        """;

    private async Task ExecuteSqlAsync(NpgsqlConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private async Task<T> ExecuteScalarAsync<T>(NpgsqlConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        var result = await command.ExecuteScalarAsync();
        return (T)Convert.ChangeType(result!, typeof(T));
    }
}
using System.Reflection;
using System.Text.RegularExpressions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MeAjudaAi.Shared.Tests.Extensions;

/// <summary>
/// Fornece descoberta automática e aplicação de migrações do Entity Framework para cenários de teste.
/// </summary>
public static class MigrationDiscoveryExtensions
{
    /// <summary>
    /// Descobre e aplica automaticamente todas as migrações pendentes para DbContexts encontrados no domínio do assembly atual.
    /// Este método escaneia todos os assemblies carregados por tipos de DbContext e aplica suas migrações.
    /// </summary>
    /// <param name="serviceProvider">O service provider contendo os DbContexts registrados</param>
    /// <param name="cancellationToken">Token de cancelamento para a operação</param>
    /// <returns>Uma task representando a operação assíncrona de migração</returns>
    public static async Task ApplyAllDiscoveredMigrationsAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        var dbContextTypes = DiscoverDbContextTypes;

        foreach (var contextType in dbContextTypes)
        {
            if (serviceProvider.GetService(contextType) is not DbContext context)
                continue;

            context.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

            await EnsureDatabaseAndMigrateAsync(context, contextType, cancellationToken);
        }
    }

    /// <summary>
    /// Garante que o banco de dados existe e aplica migrations de forma segura.
    /// Extrai a lógica compartilhada entre ApplyAllDiscoveredMigrationsAsync e EnsureAllDatabasesCreatedAsync.
    /// </summary>
    private static async Task EnsureDatabaseAndMigrateAsync(
        DbContext context,
        Type contextType,
        CancellationToken cancellationToken)
    {
        // Garante que o banco de dados existe ANTES de tentar criar schemas ou aplicar migrações
        var connectionString = context.Database.GetConnectionString();
        if (!string.IsNullOrEmpty(connectionString))
        {
            var csBuilder = new NpgsqlConnectionStringBuilder(connectionString);

            // Garante que temos uma senha - se a connection string não incluir uma,
            // usa a senha padrão do Testcontainers
            if (string.IsNullOrEmpty(csBuilder.Password))
            {
                csBuilder.Password = "test123";
            }

            var databaseName = csBuilder.Database;

            // Valida o nome do banco contra o formato permitido (alfanumérico + underscore + hífen)
            if (!IsValidDatabaseName(databaseName))
            {
                throw new InvalidOperationException(
                    $"Database name '{databaseName}' contains invalid characters. Only alphanumeric, underscore, and hyphen are allowed.");
            }

            const int maxDbCreateRetries = 5;

            for (int attempt = 1; attempt <= maxDbCreateRetries; attempt++)
            {
                try
                {
                    // Tenta conectar diretamente ao banco de dados alvo
                    await using var connTest = new NpgsqlConnection(csBuilder.ConnectionString);
                    await connTest.OpenAsync(cancellationToken);
                    connTest.Close();
                    break;
                }
                catch (NpgsqlException pgEx) when (pgEx.SqlState == "3D000") // Banco de dados não existe
                {
                    // Tenta criá-lo via banco postgres
                    try
                    {
                        // Preserva todas as credenciais ao criar a connection string do postgres
                        var postgresBuilder = new NpgsqlConnectionStringBuilder
                        {
                            Host = csBuilder.Host,
                            Port = csBuilder.Port,
                            Username = csBuilder.Username,
                            Password = csBuilder.Password,
                            Database = "postgres",
                            SslMode = csBuilder.SslMode
                        };

                        await using var connPostgres = new NpgsqlConnection(postgresBuilder.ConnectionString);
                        await connPostgres.OpenAsync(cancellationToken);

                        // Usa query parametrizada para verificar existência do banco
                        await using var checkCmd = connPostgres.CreateCommand();
                        checkCmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @dbname";
                        checkCmd.Parameters.AddWithValue("@dbname", databaseName!);
                        var exists = await checkCmd.ExecuteScalarAsync(cancellationToken);

                        if (exists is null)
                        {
                            // Para CREATE DATABASE, identificadores devem usar strings brutas (não parametrizadas)
                            await using var createCmd = connPostgres.CreateCommand();
                            createCmd.CommandText = $"CREATE DATABASE \"{databaseName}\"";
                            await createCmd.ExecuteNonQueryAsync(cancellationToken);
                        }

                        connPostgres.Close();
                        break;
                    }
                    catch (Exception createEx) when (attempt < maxDbCreateRetries && (createEx is NpgsqlException || createEx is TimeoutException))
                    {
                        await Task.Delay(500 * attempt, cancellationToken);
                    }
                }
                catch (Exception ex) when (attempt < maxDbCreateRetries && (ex is NpgsqlException || ex is TimeoutException))
                {
                    await Task.Delay(500 * attempt, cancellationToken);
                }
            }
        }

        // Cria schema DEPOIS de garantir que o banco existe
        var schema = DbContextSchemaHelper.GetSchemaName(contextType);
        if (schema != "public")
        {
            await context.Database.ExecuteSqlRawAsync(
                $"CREATE SCHEMA IF NOT EXISTS \"{schema}\";", cancellationToken);
        }

        // Aplica migrações com lógica de retry
        const int maxRetries = 5;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync(cancellationToken);
                break;
            }
            catch (Exception ex) when (
                ex is PostgresException pgEx &&
                (pgEx.SqlState == "23505" || pgEx.SqlState == "42P01") &&
                attempt < maxRetries)
            {
                await Task.Delay(100 * attempt, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Valida que o nome do banco de dados contém apenas caracteres seguros.
    /// </summary>
    private static bool IsValidDatabaseName(string? databaseName)
    {
        if (string.IsNullOrEmpty(databaseName))
            return false;

        // Permite alfanumérico, underscore e hífen; máximo 63 caracteres (limite do PostgreSQL)
        return Regex.IsMatch(databaseName, @"^[a-zA-Z0-9_-]{1,63}$");
    }

    /// <summary>
    /// Descobre todos os tipos de DbContext em assemblies carregados que seguem a convenção de nome de módulo.
    /// </summary>
    /// <returns>Um enumerable de tipos DbContext encontrados em assemblies de módulos</returns>
    private static IEnumerable<Type> DiscoverDbContextTypes
    {
        get
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic &&
                                  (assembly.FullName?.Contains("MeAjudaAi") == true ||
                                   assembly.FullName?.Contains("Users") == true ||
                                   assembly.FullName?.Contains("Infrastructure") == true));

            var dbContextTypes = new List<Type>();

            foreach (var assembly in loadedAssemblies)
            {
                try
                {
                    var contextTypes = assembly.GetTypes()
                        .Where(type => type.IsClass &&
                                      !type.IsAbstract &&
                                      typeof(DbContext).IsAssignableFrom(type) &&
                                      type.Name.EndsWith("DbContext"))
                        .ToList();

                    dbContextTypes.AddRange(contextTypes);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // Trata assemblies que não podem ser totalmente carregados
                    var loadableTypes = ex.Types.Where(t => t != null);
                    var contextTypes = loadableTypes
                        .Where(type => type!.IsClass &&
                                      !type.IsAbstract &&
                                      typeof(DbContext).IsAssignableFrom(type) &&
                                      type.Name.EndsWith("DbContext"))
                        .ToList();

                    dbContextTypes.AddRange(contextTypes!);
                }
                catch (Exception)
                {
                    // Continua com outros assemblies em caso de falha na descoberta
                }
            }

            return dbContextTypes;
        }
    }

    /// <summary>
    /// Garante que todos os DbContexts descobertos tenham seus bancos criados e migrados.
    /// Útil para preparação de testes de integração.
    /// </summary>
    /// <param name="serviceProvider">O service provider contendo os DbContexts registrados</param>
    /// <param name="cancellationToken">Token de cancelamento para a operação</param>
    /// <returns>Uma task representando a operação assíncrona</returns>
    public static async Task EnsureAllDatabasesCreatedAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        var dbContextTypes = DiscoverDbContextTypes;

        foreach (var contextType in dbContextTypes)
        {
            if (serviceProvider.GetService(contextType) is not DbContext context)
                continue;

            context.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

            await EnsureDatabaseAndMigrateAsync(context, contextType, cancellationToken);
        }
    }

    /// <summary>
    /// Obtém todos os tipos de DbContext descobertos para fins de diagnóstico.
    /// </summary>
    /// <returns>Uma lista de nomes de tipos DbContext que foram descobertos</returns>
    public static IEnumerable<string> GetDiscoveredDbContextNames()
    {
        return DiscoverDbContextTypes.Select(t => t.FullName ?? t.Name);
    }
}

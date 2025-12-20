using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// Cache inteligente de schema de banco de dados para otimizar testes de integração
/// Evita recriação desnecessária de estruturas quando o schema não mudou
/// </summary>
public class DatabaseSchemaCacheService(ILogger<DatabaseSchemaCacheService> logger)
{
    private static readonly ConcurrentDictionary<string, DatabaseSchemaInfo> SchemaCache = new();
    private static readonly SemaphoreSlim CacheLock = new(1, 1);

    /// <summary>
    /// Verifica se o schema atual é o mesmo do cache e se pode reutilizar a estrutura
    /// </summary>
    public async Task<bool> CanReuseSchemaAsync(string connectionString, string moduleName)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(moduleName);

        await CacheLock.WaitAsync();
        try
        {
            var currentSchemaHash = await CalculateCurrentSchemaHashAsync(connectionString, moduleName);
            var cacheKey = GetCacheKey(connectionString, moduleName);

            if (SchemaCache.TryGetValue(cacheKey, out var cachedInfo))
            {
                var canReuse = cachedInfo.SchemaHash == currentSchemaHash &&
                              cachedInfo.CreatedAt > DateTime.UtcNow.AddMinutes(-30); // Cache válido por 30 min

                if (canReuse)
                {
                    logger.LogInformation("[SchemaCache] Reutilizando schema existente para módulo {Module}", moduleName);
                    return true;
                }
            }

            // Atualizar cache com novo schema
            SchemaCache[cacheKey] = new DatabaseSchemaInfo
            {
                SchemaHash = currentSchemaHash,
                CreatedAt = DateTime.UtcNow,
                ModuleName = moduleName
            };

            logger.LogInformation("[SchemaCache] Schema atualizado no cache para módulo {Module}", moduleName);
            return false;
        }
        finally
        {
            CacheLock.Release();
        }
    }

    /// <summary>
    /// Marca um schema como inicializado com sucesso
    /// </summary>
    public async Task MarkSchemaAsInitializedAsync(string connectionString, string moduleName)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(moduleName);

        await CacheLock.WaitAsync();
        try
        {
            var cacheKey = GetCacheKey(connectionString, moduleName);
            if (SchemaCache.TryGetValue(cacheKey, out var info))
            {
                info.IsInitialized = true;
                info.LastSuccessfulInit = DateTime.UtcNow;
            }
        }
        finally
        {
            CacheLock.Release();
        }
    }

    /// <summary>
    /// Invalida o cache para forçar recriação (útil para testes específicos)
    /// </summary>
    public static void InvalidateCache(string connectionString, string moduleName)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(moduleName);

        var cacheKey = GetCacheKey(connectionString, moduleName);
        SchemaCache.TryRemove(cacheKey, out _);
    }

    /// <summary>
    /// Limpa todo o cache (útil entre test runs)
    /// </summary>
    public static void ClearCache()
    {
        SchemaCache.Clear();
    }

    private Task<string> CalculateCurrentSchemaHashAsync(string connectionString, string moduleName)
    {
        // Para simplificar, vamos usar um hash baseado em:
        // 1. Timestamp dos arquivos de migration mais recentes
        // 2. Nome do módulo
        // 3. ConnectionString (para distinguir diferentes bancos)

        var hashInputs = new List<string>
        {
            moduleName,
            connectionString.GetHashCode().ToString() // Simplificado para não expor connection string
        };

        // Adicionar info dos arquivos de migration se existirem
        var migrationPaths = new[]
        {
            Path.Combine("src", "Modules", moduleName, "Infrastructure", "Migrations"),
            Path.Combine("src", "Shared", "MeAjudai.Shared", "Database", "Migrations")
        };

        foreach (var path in migrationPaths)
        {
            if (Directory.Exists(path))
            {
                var migrationFiles = Directory.GetFiles(path, "*.cs")
                    .OrderByDescending(File.GetLastWriteTimeUtc)
                    .Take(5); // Apenas os 5 mais recentes para performance

                foreach (var file in migrationFiles)
                {
                    hashInputs.Add($"{Path.GetFileName(file)}:{File.GetLastWriteTimeUtc(file):O}");
                }
            }
        }

        // Gerar hash MD5 dos inputs
        var combined = string.Join("|", hashInputs);
        var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(combined));
        return Task.FromResult(Convert.ToHexString(hashBytes));
    }

    private static string GetCacheKey(string connectionString, string moduleName)
    {
        // Usar hash da connection string para não vazar informações sensíveis
        var connHash = connectionString.GetHashCode();
        return $"{moduleName}_{connHash}";
    }
}

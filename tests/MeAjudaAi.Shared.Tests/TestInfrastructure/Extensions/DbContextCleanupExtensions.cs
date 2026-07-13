using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Extensions;

/// <summary>
/// Extension methods for cleaning up DbContext data in tests.
/// Extracted from E2E.TestContainerFixture for reuse across test projects.
/// </summary>
public static class DbContextCleanupExtensions
{
    /// <summary>
    /// Truncates all tables in the given DbContext's model, respecting schemas.
    /// </summary>
    public static async Task CleanupContextAsync<TContext>(this IServiceProvider services) where TContext : DbContext
    {
        var context = services.GetRequiredService<TContext>();
        var tableNames = new List<string>();

        foreach (var entityType in context.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            var schema = entityType.GetSchema();

            if (!string.IsNullOrEmpty(tableName))
            {
                var qualifiedTableName = string.IsNullOrEmpty(schema) || schema == "public"
                    ? $"\"{tableName}\""
                    : $"\"{schema}\".\"{tableName}\"";

                tableNames.Add(qualifiedTableName);
            }
        }

        if (tableNames.Count > 0)
        {
            var uniqueTables = tableNames.Distinct().ToList();
            var batchSql = $"TRUNCATE TABLE {string.Join(", ", uniqueTables)} CASCADE";
            await context.Database.ExecuteSqlRawAsync(batchSql);
        }
    }

    /// <summary>
    /// Truncates all tables for multiple DbContext types.
    /// </summary>
    public static async Task CleanupContextsAsync(this IServiceProvider services, params Type[] contextTypes)
    {
        foreach (var contextType in contextTypes)
        {
            var method = typeof(DbContextCleanupExtensions)
                .GetMethod(nameof(CleanupContextAsync))!
                .MakeGenericMethod(contextType);

            await (Task)method.Invoke(null, [services])!;
        }
    }
}

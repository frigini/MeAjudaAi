using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MeAjudaAi.Shared.Database.Exceptions;

/// <summary>
/// Utilitário para converter exceções do PostgreSQL em exceções tipadas
/// </summary>
public static class PostgreSqlExceptionProcessor
{
    /// <summary>
    /// Processa uma DbUpdateException e tenta converter em uma exceção tipada
    /// </summary>
    public static Exception ProcessException(DbUpdateException dbUpdateException)
    {
        ArgumentNullException.ThrowIfNull(dbUpdateException);

        return dbUpdateException.InnerException is PostgresException postgresException
            ? postgresException.SqlState switch
            {
                "23505" => CreateUniqueConstraintException(postgresException), // unique_violation
                "23502" => CreateNotNullConstraintException(postgresException), // not_null_violation
                "23503" => CreateForeignKeyConstraintException(postgresException), // foreign_key_violation
                _ => dbUpdateException
            }
            : dbUpdateException;
    }

    private static UniqueConstraintException CreateUniqueConstraintException(PostgresException postgresException)
    {
        var constraintName = ExtractConstraintName(postgresException.Detail);
        var columnName = ExtractColumnName(postgresException.Detail);

        return new UniqueConstraintException(constraintName, columnName, postgresException);
    }

    private static NotNullConstraintException CreateNotNullConstraintException(PostgresException postgresException)
    {
        var columnName = ExtractColumnName(postgresException.Detail);
        return new NotNullConstraintException(columnName, postgresException, true);
    }

    private static ForeignKeyConstraintException CreateForeignKeyConstraintException(PostgresException postgresException)
    {
        var constraintName = ExtractConstraintName(postgresException.Detail);
        var tableName = ExtractTableName(postgresException.Detail);

        return new ForeignKeyConstraintException(constraintName, tableName, postgresException);
    }

    private static string? ExtractConstraintName(string? detail)
    {
        if (string.IsNullOrEmpty(detail)) return null;

        // Padrão comum: "Key (column)=(value) already exists."
        // ou "violates foreign key constraint \"constraint_name\""
        var constraintMatch = System.Text.RegularExpressions.Regex.Match(
            detail, @"constraint\s+""([^""]+)""",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return constraintMatch.Success ? constraintMatch.Groups[1].Value : null;
    }

    private static string? ExtractColumnName(string? detail)
    {
        if (string.IsNullOrEmpty(detail)) return null;

        // Padrão comum: "Key (column_name)=(value) already exists."
        var columnMatch = System.Text.RegularExpressions.Regex.Match(
            detail, @"Key\s+\(([^)]+)\)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return columnMatch.Success ? columnMatch.Groups[1].Value : null;
    }

    private static string? ExtractTableName(string? detail)
    {
        if (string.IsNullOrEmpty(detail)) return null;

        // Padrão comum: "violates foreign key constraint on table \"table_name\""
        var tableMatch = System.Text.RegularExpressions.Regex.Match(
            detail, @"table\s+""([^""]+)""",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return tableMatch.Success ? tableMatch.Groups[1].Value : null;
    }
}

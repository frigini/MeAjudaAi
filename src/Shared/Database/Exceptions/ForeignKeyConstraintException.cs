using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Shared.Database.Exceptions;

/// <summary>
/// Exceção quando uma chave estrangeira é violada
/// </summary>
public class ForeignKeyConstraintException : DbUpdateException
{
    public string? ConstraintName { get; }
    public string? TableName { get; }

    public ForeignKeyConstraintException() : base("Foreign key constraint violation") { }

    public ForeignKeyConstraintException(string message) : base(message) { }

    public ForeignKeyConstraintException(string message, Exception innerException) : base(message, innerException) { }

    public ForeignKeyConstraintException(string? constraintName, string? tableName, Exception innerException)
        : base($"Foreign key constraint violation on table {tableName ?? "unknown table"}", innerException)
    {
        ConstraintName = constraintName;
        TableName = tableName;
    }
}

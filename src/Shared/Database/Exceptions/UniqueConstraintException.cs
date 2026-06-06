using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Shared.Database.Exceptions;

/// <summary>
/// Exceção quando uma restrição única é violada
/// </summary>
public class UniqueConstraintException : DbUpdateException
{
    public string? ConstraintName { get; }
    public string? ColumnName { get; }

    public UniqueConstraintException() : base("Unique constraint violation") { }

    public UniqueConstraintException(string message) : base(message) { }

    public UniqueConstraintException(string message, Exception innerException) : base(message, innerException) { }

    public UniqueConstraintException(string? constraintName, string? columnName, Exception innerException)
        : base($"Unique constraint violation on {columnName ?? "unknown column"}", innerException)
    {
        ConstraintName = constraintName;
        ColumnName = columnName;
    }
}

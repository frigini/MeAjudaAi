using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Shared.Database.Exceptions;

/// <summary>
/// Exceção quando um valor nulo é inserido em uma coluna NOT NULL
/// </summary>
public class NotNullConstraintException : DbUpdateException
{
    public string? ColumnName { get; }

    public NotNullConstraintException() : base("Cannot insert null value") { }

    public NotNullConstraintException(string message) : base(message) { }

    public NotNullConstraintException(string message, Exception innerException) : base(message, innerException) { }

    public NotNullConstraintException(string? columnName, Exception innerException, bool isColumnName)
        : base($"Cannot insert null value into column {columnName ?? "unknown column"}", innerException)
    {
        if (isColumnName) ColumnName = columnName;
    }
}

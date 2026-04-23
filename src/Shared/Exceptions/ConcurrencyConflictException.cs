namespace MeAjudaAi.Shared.Exceptions;

/// <summary>
/// Exceção lançada quando ocorre um conflito de concorrência otimista (ex: RowVersion mismatch)
/// ou um erro de serialização de transação no banco de dados.
/// </summary>
public class ConcurrencyConflictException : Exception
{
    /// <summary>Identificador do agregado que causou o conflito, se disponível.</summary>
    public string? AggregateId { get; }

    /// <summary>Tipo da entidade que causou o conflito, se disponível.</summary>
    public string? EntityType { get; }

    public ConcurrencyConflictException() 
        : base("O registro foi modificado por outro usuário ou processo. Por favor, tente novamente.")
    {
    }

    public ConcurrencyConflictException(string message) 
        : base(message)
    {
    }

    public ConcurrencyConflictException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public ConcurrencyConflictException(string message, string? aggregateId, string? entityType)
        : base(message)
    {
        AggregateId = aggregateId;
        EntityType = entityType;
    }

    public ConcurrencyConflictException(string message, string? aggregateId, string? entityType, Exception innerException)
        : base(message, innerException)
    {
        AggregateId = aggregateId;
        EntityType = entityType;
    }
}

namespace MeAjudaAi.Shared.Database.Exceptions;

/// <summary>
/// Exceção lançada quando ocorre um conflito de concorrência otimista (ex: RowVersion mismatch).
/// </summary>
public class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException() : base("O registro foi modificado por outro usuário ou processo.") { }

    public ConcurrencyConflictException(string message) : base(message) { }

    public ConcurrencyConflictException(string message, Exception innerException) : base(message, innerException) { }
}

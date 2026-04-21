namespace MeAjudaAi.Shared.Exceptions;

/// <summary>
/// Exceção lançada quando ocorre um conflito de concorrência otimista (ex: RowVersion mismatch)
/// ou um erro de serialização de transação no banco de dados.
/// </summary>
public class ConcurrencyConflictException : Exception
{
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
}

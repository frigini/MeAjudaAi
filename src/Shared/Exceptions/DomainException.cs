namespace MeAjudaAi.Shared.Exceptions;

public abstract class DomainException : Exception
{
    /// <summary>
    /// Status HTTP intencionado para esta exceção. Quando definido, o <c>GlobalExceptionHandler</c>
    /// usa este valor em vez do padrão 400.
    /// </summary>
    public int? HttpStatusCode { get; }

    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, int httpStatusCode) : base(message)
    {
        HttpStatusCode = httpStatusCode;
    }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
    protected DomainException(string message, Exception innerException, int httpStatusCode) : base(message, innerException)
    {
        HttpStatusCode = httpStatusCode;
    }
}

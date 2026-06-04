namespace MeAjudaAi.Shared.Authorization.Exceptions;

/// <summary>
/// Exceção lançada quando ocorre um erro durante o processamento ou recuperação de permissões.
/// </summary>
public class PermissionServiceException : Exception
{
    public PermissionServiceException() : base("Ocorreu um erro no serviço de permissões.")
    {
    }

    public PermissionServiceException(string message) : base(message)
    {
    }

    public PermissionServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

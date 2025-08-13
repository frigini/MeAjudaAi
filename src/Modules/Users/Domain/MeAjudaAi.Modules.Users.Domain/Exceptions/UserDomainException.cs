using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Modules.Users.Domain.Exceptions;

public class UserDomainException : DomainException
{
    public UserDomainException(string message) : base(message)
    {
    }

    public UserDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public UserDomainException(string message, params object[] args) : base(string.Format(message, args))
    {
    }

    public UserDomainException(string message, Exception innerException, params object[] args) 
        : base(string.Format(message, args), innerException)
    {
    }
}
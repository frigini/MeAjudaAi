using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Modules.Payments.Application.Subscriptions.Exceptions;

public class SubscriptionCreationException : DomainException
{
    public SubscriptionCreationException(string message) : base(message)
    {
    }

    public SubscriptionCreationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

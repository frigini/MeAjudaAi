namespace MeAjudaAi.Modules.Payments.Application.Subscriptions.Exceptions;

public class SubscriptionCreationException : Exception
{
    public SubscriptionCreationException(string message) : base(message)
    {
    }

    public SubscriptionCreationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

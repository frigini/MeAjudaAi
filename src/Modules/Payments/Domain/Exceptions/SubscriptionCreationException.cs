using MeAjudaAi.Shared.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Payments.Domain.Exceptions;

[ExcludeFromCodeCoverage]
public class SubscriptionCreationException : DomainException
{
    public SubscriptionCreationException(string message) : base(message) { }

    public SubscriptionCreationException(string message, int httpStatusCode) : base(message, httpStatusCode) { }

    public SubscriptionCreationException(string message, Exception innerException) : base(message, innerException) { }

    public SubscriptionCreationException(string message, Exception innerException, int httpStatusCode)
        : base(message, innerException, httpStatusCode) { }
}

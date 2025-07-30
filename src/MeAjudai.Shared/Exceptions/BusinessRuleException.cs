namespace MeAjudaAi.Shared.Exceptions;

public class BusinessRuleException(string ruleName, string message) : DomainException(message)
{
    public string RuleName { get; } = ruleName;
}
using System.Diagnostics.CodeAnalysis;
namespace MeAjudaAi.Shared.Exceptions;

[ExcludeFromCodeCoverage]

public class BusinessRuleException(string ruleName, string message) : DomainException(message)
{
    public string RuleName { get; } = ruleName;
}

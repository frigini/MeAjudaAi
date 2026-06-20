using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Payments.Application.Options;

[ExcludeFromCodeCoverage]
public class PlanOptions
{
    public string? StripePriceId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
}

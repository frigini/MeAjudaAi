using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Payments.Application.Options;

[ExcludeFromCodeCoverage]
public class PaymentsOptions
{
    public const string SectionName = "Payments";

    public string? SuccessUrl { get; set; }
    public string? CancelUrl { get; set; }
    public string[] AllowedReturnHosts { get; set; } = Array.Empty<string>();
}

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MeAjudaAi.Modules.Bookings.Application.Authorization.Models;

[ExcludeFromCodeCoverage]
internal sealed record ProviderResolutionResult
{
    public Guid? ProviderId { get; init; }
    public bool IsNotLinked { get; init; }

    [JsonIgnore]
    public bool IsFound => ProviderId.HasValue;

    [JsonConstructor]
    public ProviderResolutionResult() { }

    public static ProviderResolutionResult NotLinked() => new() { IsNotLinked = true };
    public static ProviderResolutionResult Found(Guid providerId) => new() { ProviderId = providerId };
}

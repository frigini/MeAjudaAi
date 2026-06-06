using System.Diagnostics.CodeAnalysis;

using System.Text.Json.Serialization;

namespace MeAjudaAi.Shared.Authorization.Keycloak.Models;

[ExcludeFromCodeCoverage]

public sealed class KeycloakRole
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

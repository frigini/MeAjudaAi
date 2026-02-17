using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MeAjudaAi.Contracts.Configuration;

namespace MeAjudaAi.ApiService.Endpoints;

/// <summary>
/// Endpoint para fornecer configuração do cliente (não-sensível).
/// Permite que o Blazor WASM obtenha configuração do backend ao iniciar.
/// </summary>
public static class ConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapConfigurationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/configuration")
            .WithTags("Configuration");

        group.MapGet("/client", GetClientConfiguration)
            .WithName("GetClientConfiguration")
            .WithSummary("Obtém a configuração do cliente Blazor WASM")
            .WithDescription("Retorna configurações não-sensíveis necessárias para o frontend (Keycloak, URLs, feature flags)")
            .Produces<ClientConfiguration>(StatusCodes.Status200OK)
            .AllowAnonymous(); // Deve ser público para que o app possa buscar config antes de autenticar

        return endpoints;
    }

    /// <summary>
    /// Retorna a configuração do cliente.
    /// Apenas informações não-sensíveis são expostas.
    /// </summary>
    private static Ok<ClientConfiguration> GetClientConfiguration(
        [FromServices] IConfiguration configuration,
        [FromServices] IWebHostEnvironment environment)
    {
        // Obter URL base da API do host atual ou configuração
        var apiBaseUrl = configuration["ApiBaseUrl"] 
            ?? configuration["ASPNETCORE_URLS"]?.Split(';').FirstOrDefault() 
            ?? "https://localhost:7001";

        // Normalizar URL (remover trailing slash)
        apiBaseUrl = apiBaseUrl.TrimEnd('/');

        // Configuração do Keycloak - suportar tanto o novo formato (BaseUrl + Realm) quanto o legado (Authority)
        var keycloakAuthority = configuration["Keycloak:Authority"]?.TrimEnd('/');
        
        if (string.IsNullOrWhiteSpace(keycloakAuthority))
        {





        
        
            // Construir Authority a partir de BaseUrl e Realm
            var keycloakBaseUrl = configuration["Keycloak:BaseUrl"];
            if (string.IsNullOrWhiteSpace(keycloakBaseUrl))
                throw new InvalidOperationException("Keycloak:BaseUrl ou Keycloak:Authority deve estar configurado");
            
            keycloakBaseUrl = keycloakBaseUrl.TrimEnd('/');
            
            var keycloakRealm = configuration["Keycloak:Realm"];
            if (string.IsNullOrWhiteSpace(keycloakRealm))
                keycloakRealm = "meajudaai"; // Valor padrão
            
            keycloakRealm = keycloakRealm.Trim('/');

            keycloakAuthority = $"{keycloakBaseUrl}/realms/{keycloakRealm}";
        }

        var keycloakClientId = configuration["Keycloak:ClientId"] 
            ?? throw new InvalidOperationException("Keycloak:ClientId não configurado");

        // URL de logout - usa a URL base do cliente WASM
        var clientBaseUrl = configuration["ClientBaseUrl"] ?? "http://localhost:5165";
        var postLogoutRedirectUri = $"{clientBaseUrl.TrimEnd('/')}/";

        var clientConfig = new ClientConfiguration
        {
            ApiBaseUrl = apiBaseUrl,
            Keycloak = new KeycloakConfiguration
            {
                Authority = keycloakAuthority,
                ClientId = keycloakClientId,
                ResponseType = configuration["Keycloak:ResponseType"] ?? "code",
                Scope = configuration["Keycloak:Scope"] ?? "openid profile email",
                PostLogoutRedirectUri = postLogoutRedirectUri
            },
            External = new ExternalResources
            {
                DocumentationUrl = configuration["External:DocumentationUrl"],
                SupportUrl = configuration["External:SupportUrl"]
            },
            Features = new FeatureFlags
            {
                EnableReduxDevTools = environment.IsDevelopment(),
                EnableDebugMode = environment.IsDevelopment(),
                EnableFakeAuth = configuration.GetValue<bool>("FeatureFlags:EnableFakeAuth")
            }
        };

        return TypedResults.Ok(clientConfig);
    }
}

using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using FluentValidation;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Shared.Contracts.Configuration;
using MeAjudaAi.Web.Admin;
using MeAjudaAi.Web.Admin.Extensions;
using MeAjudaAi.Web.Admin.Services;
using MeAjudaAi.Web.Admin.Validators;
using MudBlazor;
using MudBlazor.Services;
using System.Net.Http.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ====================================
// STEP 1: Fetch Configuration from Backend
// ====================================
// Create temporary HttpClient to fetch configuration
// Use fallback API URL from local config or default
var temporaryApiUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;

using var tempClient = new HttpClient { BaseAddress = new Uri(temporaryApiUrl) };

ClientConfiguration clientConfig;
try
{
    Console.WriteLine($"🔧 Fetching configuration from: {temporaryApiUrl}/api/configuration/client");
    
    var response = await tempClient.GetAsync("/api/configuration/client");
    
    if (!response.IsSuccessStatusCode)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException(
            $"❌ Failed to fetch configuration from backend.\n" +
            $"Status: {response.StatusCode}\n" +
            $"Error: {errorContent}\n" +
            $"API URL: {temporaryApiUrl}\n\n" +
            $"Please ensure:\n" +
            $"  1. The API backend is running\n" +
            $"  2. The API URL is correct in appsettings.json\n" +
            $"  3. CORS is configured for this origin");
    }

    clientConfig = await response.Content.ReadFromJsonAsync<ClientConfiguration>()
        ?? throw new InvalidOperationException("❌ Configuration endpoint returned null");

    Console.WriteLine($"✅ Configuration loaded successfully");
    Console.WriteLine($"   API Base URL: {clientConfig.ApiBaseUrl}");
    Console.WriteLine($"   Keycloak Authority: {clientConfig.Keycloak.Authority}");
    Console.WriteLine($"   Keycloak Client ID: {clientConfig.Keycloak.ClientId}");
}
catch (HttpRequestException ex)
{
    throw new InvalidOperationException(
        $"❌ Cannot connect to the backend API to fetch configuration.\n" +
        $"API URL: {temporaryApiUrl}\n\n" +
        $"Please ensure the API backend is running and accessible.\n" +
        $"Original error: {ex.Message}", ex);
}
catch (Exception ex)
{
    throw new InvalidOperationException(
        $"❌ Failed to load application configuration from backend.\n" +
        $"Error: {ex.Message}", ex);
}

// ====================================
// STEP 2: Validate Configuration
// ====================================
ValidateConfiguration(clientConfig);

// ====================================
// STEP 3: Register Services with Configuration
// ====================================

// Registrar handler de autenticação customizado
builder.Services.AddScoped<ApiAuthorizationMessageHandler>();

// Configuração do HttpClient com autenticação usando URL do backend
builder.Services.AddHttpClient("MeAjudaAi.API", client => 
        client.BaseAddress = new Uri(clientConfig.ApiBaseUrl))
    .AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("MeAjudaAi.API"));

// Autenticação Keycloak OIDC com configuração do backend
builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.Authority = clientConfig.Keycloak.Authority;
    options.ProviderOptions.ClientId = clientConfig.Keycloak.ClientId;
    options.ProviderOptions.ResponseType = clientConfig.Keycloak.ResponseType;
    options.ProviderOptions.DefaultScopes.Clear();
    
    // Add scopes from configuration
    foreach (var scope in clientConfig.Keycloak.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries))
    {
        options.ProviderOptions.DefaultScopes.Add(scope);
    }
    
    options.ProviderOptions.PostLogoutRedirectUri = clientConfig.Keycloak.PostLogoutRedirectUri;
    options.UserOptions.RoleClaim = "roles";
});

// Refit API clients usando URL do backend
builder.Services
    .AddApiClient<IProvidersApi>(clientConfig.ApiBaseUrl)
    .AddApiClient<IServiceCatalogsApi>(clientConfig.ApiBaseUrl)
    .AddApiClient<ILocationsApi>(clientConfig.ApiBaseUrl)
    .AddApiClient<IDocumentsApi>(clientConfig.ApiBaseUrl);

// Registrar ClientConfiguration como singleton para uso em componentes
builder.Services.AddSingleton(clientConfig);

// Serviços MudBlazor
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
});

// FluentValidation - Registrar validadores
builder.Services.AddValidatorsFromAssemblyContaining<CreateProviderRequestDtoValidator>();

// Gerenciamento de estado Fluxor
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
    
    // Enable Redux DevTools based on feature flag from backend
    if (clientConfig.Features.EnableReduxDevTools)
    {
        options.UseReduxDevTools();
    }
});

Console.WriteLine("🚀 Starting MeAjudaAi Admin Portal");
await builder.Build().RunAsync();

// ====================================
// Helper Methods
// ====================================

static void ValidateConfiguration(ClientConfiguration config)
{
    var errors = new List<string>();

    if (string.IsNullOrWhiteSpace(config.ApiBaseUrl))
        errors.Add("❌ ApiBaseUrl is missing");

    if (string.IsNullOrWhiteSpace(config.Keycloak.Authority))
        errors.Add("❌ Keycloak Authority is missing");

    if (string.IsNullOrWhiteSpace(config.Keycloak.ClientId))
        errors.Add("❌ Keycloak ClientId is missing");

    if (string.IsNullOrWhiteSpace(config.Keycloak.PostLogoutRedirectUri))
        errors.Add("❌ Keycloak PostLogoutRedirectUri is missing");

    if (!Uri.TryCreate(config.ApiBaseUrl, UriKind.Absolute, out _))
        errors.Add("❌ ApiBaseUrl is not a valid absolute URI");

    if (!Uri.TryCreate(config.Keycloak.Authority, UriKind.Absolute, out _))
        errors.Add("❌ Keycloak Authority is not a valid absolute URI");

    if (errors.Any())
    {
        var errorMessage = "\n❌❌❌ CONFIGURATION VALIDATION FAILED ❌❌❌\n\n" +
            string.Join("\n", errors) +
            "\n\nPlease check your backend configuration and ensure all required settings are properly configured.\n";
        
        Console.Error.WriteLine(errorMessage);
        throw new InvalidOperationException(errorMessage);
    }

    Console.WriteLine("✅ Configuration validation passed");
}

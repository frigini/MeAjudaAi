using FluentValidation;
using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Contracts.Configuration;
using MeAjudaAi.Web.Admin;
using MeAjudaAi.Web.Admin.Authentication;
using MeAjudaAi.Web.Admin.Authorization;
using MeAjudaAi.Web.Admin.Extensions;
using MeAjudaAi.Web.Admin.Services;
using MeAjudaAi.Web.Admin.Services.Interfaces;
using MeAjudaAi.Web.Admin.Services.Resilience.Http;
using MeAjudaAi.Web.Admin.Services.Resilience.Interfaces;
using MeAjudaAi.Web.Admin.Validators;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using System.Globalization;
using System.Net.Http.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ====================================
// PASSO 1: Buscar Configuração do Backend
// ====================================
// Criar HttpClient temporário para buscar configuração
// Usar URL da API de fallback da configuração local ou padrão
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
// PASSO 2: Validar Configuração
// ====================================
ValidateConfiguration(clientConfig);

// ====================================
// PASSO 3: Registrar Serviços com Configuração
// ====================================

// Registrar serviço de status de conexão (singleton para compartilhar estado)
builder.Services.AddSingleton<IConnectionStatusService, ConnectionStatusService>();

// Registrar handlers de resiliência
builder.Services.AddScoped<PollyLoggingHandler>();

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
    
    // Adicionar configurações avançadas do OIDC para melhorar compatibilidade
    // Estas configurações ajudam a lidar com provedores que não seguem completamente a spec OIDC
    options.ProviderOptions.MetadataUrl = $"{clientConfig.Keycloak.Authority}/.well-known/openid-configuration";
    
    // Adicionar scopes da configuração
    if (!string.IsNullOrWhiteSpace(clientConfig.Keycloak.Scope))
    {
        options.ProviderOptions.DefaultScopes.Clear();
        foreach (var scope in clientConfig.Keycloak.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            options.ProviderOptions.DefaultScopes.Add(scope);
        }
    }
    
    options.ProviderOptions.PostLogoutRedirectUri = clientConfig.Keycloak.PostLogoutRedirectUri;
    options.UserOptions.RoleClaim = "role"; // Mudado para "role" pois vamos converter para ClaimTypes.Role
})
.AddAccountClaimsPrincipalFactory<CustomAccountClaimsPrincipalFactory>();

// Autorização com políticas baseadas em roles
builder.Services.AddAuthorizationCore(options =>
{
    // Política de Admin - requer role "admin"
    options.AddPolicy(PolicyNames.AdminPolicy, policy =>
        policy.RequireRole(RoleNames.Admin));

    // Política de Gerente de Provedores - requer "provider-manager" ou "admin"
    options.AddPolicy(PolicyNames.ProviderManagerPolicy, policy =>
        policy.RequireRole(RoleNames.ProviderManager, RoleNames.Admin));

    // Política de Revisor de Documentos - requer "document-reviewer" ou "admin"
    options.AddPolicy(PolicyNames.DocumentReviewerPolicy, policy =>
        policy.RequireRole(RoleNames.DocumentReviewer, RoleNames.Admin));

    // Política de Gerente de Catálogo - requer "catalog-manager" ou "admin"
    options.AddPolicy(PolicyNames.CatalogManagerPolicy, policy =>
        policy.RequireRole(RoleNames.CatalogManager, RoleNames.Admin));

    // Política de Visualizador - qualquer usuário autenticado
    options.AddPolicy(PolicyNames.ViewerPolicy, policy =>
        policy.RequireAuthenticatedUser());

    // Política de Gerente de Localidades - requer "locations-manager" ou "admin"
    options.AddPolicy(PolicyNames.LocationsManagerPolicy, policy =>
        policy.RequireRole(RoleNames.LocationsManager, RoleNames.Admin));
});

// Registrar serviço de permissões
builder.Services.AddScoped<IPermissionService, PermissionService>();

// Registrar serviços de diagnóstico e debug
builder.Services.AddScoped<OidcDebugService>();

// Registrar serviços de acessibilidade e error handling
builder.Services.AddScoped<LiveRegionService>();
builder.Services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();
builder.Services.AddScoped<ErrorLoggingService>();
builder.Services.AddScoped<ErrorHandlingService>();

// ====================================
// LOCALIZAÇÃO (.resx com IStringLocalizer)
// ====================================
builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

// Set default culture (will be overridden by localStorage in App.razor)
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

// Clientes de API (Refit) com políticas Polly de resiliência
builder.Services
    .AddApiClient<IProvidersApi>(clientConfig.ApiBaseUrl)
    .AddApiClient<IServiceCatalogsApi>(clientConfig.ApiBaseUrl)
    .AddApiClient<ILocationsApi>(clientConfig.ApiBaseUrl)
    .AddApiClient<IDocumentsApi>(clientConfig.ApiBaseUrl, useUploadPolicy: true); // Upload usa política sem retry

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
// Métodos Auxiliares
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

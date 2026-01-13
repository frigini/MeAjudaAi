using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Web.Admin;
using MeAjudaAi.Web.Admin.Extensions;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// URL base da API - obtém da configuração injetada pelo Aspire ou fallback para appsettings
var apiBaseUrl = builder.Configuration["services:apiservice:https:0"] 
    ?? builder.Configuration["services:apiservice:http:0"]
    ?? builder.Configuration["ApiBaseUrl"] 
    ?? builder.HostEnvironment.BaseAddress;

// Configuração do HttpClient com autenticação
builder.Services.AddHttpClient("MeAjudaAi.API", client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("MeAjudaAi.API"));

// Autenticação Keycloak OIDC
builder.Services.AddOidcAuthentication(options =>
{
    // Usar URL do Keycloak injetada pelo Aspire ou fallback para appsettings
    var keycloakUrl = builder.Configuration["services:keycloak:http:0"] 
        ?? builder.Configuration["Keycloak:Authority"];
    
    if (!string.IsNullOrEmpty(keycloakUrl))
    {
        options.ProviderOptions.Authority = $"{keycloakUrl}/realms/meajudaai";
    }
    
    builder.Configuration.Bind("Keycloak", options.ProviderOptions);
    options.UserOptions.RoleClaim = "roles";
});

// Refit API clients
builder.Services
    .AddApiClient<IProvidersApi>(apiBaseUrl)
    .AddApiClient<IServiceCatalogsApi>(apiBaseUrl)
    .AddApiClient<ILocationsApi>(apiBaseUrl)
    .AddApiClient<IDocumentsApi>(apiBaseUrl);

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

// Gerenciamento de estado Fluxor
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
#if DEBUG
    options.UseReduxDevTools();
#endif
});

await builder.Build().RunAsync();

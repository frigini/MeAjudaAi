using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Web.Admin;
using MeAjudaAi.Web.Admin.Extensions;
using MeAjudaAi.Web.Admin.Services;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// URL base da API
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;

// Registrar handler de autenticação customizado
builder.Services.AddScoped<ApiAuthorizationMessageHandler>();

// Configuração do HttpClient com autenticação
builder.Services.AddHttpClient("MeAjudaAi.API", client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("MeAjudaAi.API"));

// Autenticação Keycloak OIDC
builder.Services.AddOidcAuthentication(options =>
{
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

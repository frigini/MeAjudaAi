using MeAjudaAi.ServiceDefaults;
using MeAjudaAi.Gateway.Options;
using MeAjudaAi.Gateway.Middlewares;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

// Adiciona ServiceDefaults (Logging, Metrics, Service Discovery)
builder.AddServiceDefaults();

// Configurações de Opções
builder.Services.Configure<GeographicRestrictionOptions>(
    builder.Configuration.GetSection(GeographicRestrictionOptions.SectionName));

// Registrar Serviços de Geolocalização (Shared)
builder.Services.AddHttpClient<IGeographicValidationService, IbgeGeographicValidationService>();
builder.Services.AddMemoryCache();
builder.Services.AddFeatureManagement();

// Configura YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// CORS e Rate Limiting também devem ser configurados aqui futuramente
// Por enquanto vamos focar na estrutura base e Geoblocking

var app = builder.Build();

app.MapDefaultEndpoints();

// Middlewares na ordem correta
app.UseMiddleware<GeographicRestrictionMiddleware>();

// Habilita YARP
app.MapReverseProxy();

app.Run();

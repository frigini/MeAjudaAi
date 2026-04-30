using System.Text.Json;
using MeAjudaAi.Gateway.Options;
using MeAjudaAi.ServiceDefaults;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.FeatureManagement;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<GatewayCorsOptions>(
    builder.Configuration.GetSection(GatewayCorsOptions.SectionName));
builder.Services.Configure<GeographicRestrictionOptions>(
    builder.Configuration.GetSection(GeographicRestrictionOptions.SectionName));
builder.Services.Configure<RateLimitingOptions>(
    builder.Configuration.GetSection(RateLimitingOptions.SectionName));

builder.Services.AddMemoryCache();
builder.Services.AddFeatureManagement();

var corsConfig = builder.Configuration.GetSection(GatewayCorsOptions.SectionName).Get<GatewayCorsOptions>() ?? new GatewayCorsOptions();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (corsConfig.AllowedOrigins.Contains("*"))
        {
            policy.SetIsOriginAllowed(_ => true);
        }
        else
        {
            policy.WithOrigins(corsConfig.AllowedOrigins.ToArray());
        }

        policy.WithMethods(corsConfig.AllowedMethods.ToArray());

        if (corsConfig.AllowedHeaders.Contains("*"))
        {
            policy.AllowAnyHeader();
        }
        else
        {
            policy.WithHeaders(corsConfig.AllowedHeaders.ToArray());
        }

        if (corsConfig.AllowCredentials)
        {
            policy.AllowCredentials();
        }

        policy.SetPreflightMaxAge(TimeSpan.FromSeconds(corsConfig.MaxAgeSeconds));
    });
});

var keycloakBaseUrl = builder.Configuration["Keycloak:BaseUrl"] ?? "http://localhost:8080";
var keycloakRealm = builder.Configuration["Keycloak:Realm"] ?? "meajudaai";
var keycloakClientId = builder.Configuration["Keycloak:ClientId"] ?? "admin-portal";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"{keycloakBaseUrl}/realms/{keycloakRealm}";
        options.Audience = keycloakClientId;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization();

var timeoutSeconds = builder.Configuration.GetValue<int>("GatewayResilience:TimeoutSeconds", 30);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transforms =>
    {
        transforms.AddRequestTransform(async context =>
        {
            context.ProxyRequest.Headers.Add("X-Forwarded-For", context.HttpContext.Connection.RemoteIpAddress?.ToString());
            context.ProxyRequest.Headers.Add("X-Forwarded-Proto", context.HttpContext.Request.Scheme);
            context.ProxyRequest.Headers.Add("X-Original-Host", context.HttpContext.Request.Host.ToString());
            context.ProxyRequest.Headers.Add("X-Gateway-Name", "MeAjudaAi-Gateway");
            context.ProxyRequest.Headers.Add("X-Request-Timeout", timeoutSeconds.ToString());
        });
    });

var app = builder.Build();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<RateLimitingMiddleware>();

app.MapDefaultEndpoints();

app.UseMiddleware<GeographicRestrictionMiddleware>();

app.MapReverseProxy();

app.Run();
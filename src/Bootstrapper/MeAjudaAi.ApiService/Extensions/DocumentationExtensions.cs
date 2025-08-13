using MeAjudaAi.ApiService.Filters;
using Microsoft.OpenApi.Models;

namespace MeAjudaAi.ApiService.Extensions;

public static class DocumentationExtensions
{
    public static IServiceCollection AddDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(n => n.FullName);

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MeAjudaAi API",
                Version = "v1",
                Description = "API para busca e contratação de prestadores de serviço - Versão 1.0",
                Contact = new OpenApiContact
                {
                    Name = "MeAjudaAi Team",
                    Email = "contato@MeAjudaAi.com"
                }
            });


            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            options.EnableAnnotations();
            options.DocInclusionPredicate((name, api) => true);

            options.OperationFilter<ApiVersionOperationFilter>();
        });

        return services;
    }
}
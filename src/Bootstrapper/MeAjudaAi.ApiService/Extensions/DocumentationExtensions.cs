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
            // Resolver conflitos de schema entre versões
            options.CustomSchemaIds(type => type.FullName);

            // Configurar documentação para v1.0
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MeAjudaAi API",
                Version = "v1.0",
                Description = """
                    API para gerenciamento de usuários e prestadores de serviço.
                    
                    **Características:**
                    - Arquitetura CQRS com cache automático
                    - Rate limiting por usuário (60-500 req/min)
                    - Autenticação JWT/Keycloak
                    - Validação automática com FluentValidation
                    - Versionamento via URL (/api/v1/), Header (Api-Version) ou Query (?api-version=1.0)
                    
                    **Rate Limits:**
                    - Anônimos: 60/min | Autenticados: 200/min | Admins: 500/min
                    
                    **Versionamento:**
                    - URL: `/api/v1/users` (principal)
                    - Header: `Api-Version: 1.0` (alternativo)
                    - Query: `?api-version=1.0` (opcional)
                    """,
                Contact = new OpenApiContact
                {
                    Name = "MeAjudaAi Team",
                    Email = "dev@meajudaai.com"
                }
            });

            // Configuração de autenticação JWT
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using Bearer scheme. Example: 'Bearer {token}'"
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

            // Incluir comentários XML se disponíveis
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
            foreach (var xmlFile in xmlFiles)
            {
                try
                {
                    options.IncludeXmlComments(xmlFile);
                }
                catch
                {
                    // Ignora erros de XML inválido
                }
            }

            options.EnableAnnotations();

            // Configurações avançadas para melhor documentação
            options.UseInlineDefinitionsForEnums();
            options.DescribeAllParametersInCamelCase();
            options.CustomOperationIds(apiDesc =>
            {
                // Gerar IDs únicos para cada operação - suporte para minimal APIs
                var routeValues = apiDesc.ActionDescriptor.RouteValues;
                var httpMethod = apiDesc.HttpMethod ?? "Unknown";
                
                string controllerName = "Unknown";
                string actionName = "Unknown";
                
                // Tentar obter controller e action dos RouteValues (para controllers MVC)
                if (routeValues.TryGetValue("controller", out var controller) && !string.IsNullOrEmpty(controller))
                {
                    controllerName = controller;
                }
                
                if (routeValues.TryGetValue("action", out var action) && !string.IsNullOrEmpty(action))
                {
                    actionName = action;
                }
                
                // Fallback para minimal APIs ou quando RouteValues não estão disponíveis
                if (controllerName == "Unknown" || actionName == "Unknown")
                {
                    // Tentar usar ActionDescriptor para obter informações do método
                    var actionDescriptor = apiDesc.ActionDescriptor;
                    if (actionDescriptor != null)
                    {
                        // Para controllers MVC tradicionais
                        if (actionDescriptor.DisplayName != null && actionDescriptor.DisplayName.Contains('.'))
                        {
                            var parts = actionDescriptor.DisplayName.Split('.');
                            if (parts.Length >= 2)
                            {
                                controllerName = parts[^2]; // Penúltimo elemento
                                actionName = parts[^1].Split(' ')[0]; // Primeiro token do último elemento
                            }
                        }
                        else
                        {
                            // Último recurso: usar RelativePath e HttpMethod
                            var pathSegments = apiDesc.RelativePath?.Split('/')
                                .Where(s => !string.IsNullOrEmpty(s) && !s.StartsWith("{"))
                                .ToArray();
                            
                            controllerName = pathSegments?.FirstOrDefault() ?? "Api";
                            actionName = pathSegments?.LastOrDefault() ?? httpMethod;
                        }
                    }
                }
                
                return $"{controllerName}_{actionName}_{httpMethod}";
            });

            // Exemplos automáticos baseados em annotations
            options.SchemaFilter<ExampleSchemaFilter>();

            // Filtros essenciais
            options.OperationFilter<ApiVersionOperationFilter>();
            options.DocumentFilter<ModuleTagsDocumentFilter>();
        });

        return services;
    }

    public static IApplicationBuilder UseDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "api-docs/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("v1/swagger.json", "MeAjudaAi API v1.0");
            options.RoutePrefix = "api-docs";
            options.DocumentTitle = "MeAjudaAi API";

            // Configurações essenciais de UI
            options.DefaultModelsExpandDepth(1);
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            options.EnableDeepLinking();
            options.EnableFilter();

            // CSS otimizado
            options.InjectStylesheet("/css/swagger-custom.css");
        });

        return app;
    }
}
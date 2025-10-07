using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MeAjudaAi.ApiService.Filters;

/// <summary>
/// Filtro para organizar tags por módulos e adicionar descrições
/// </summary>
public class ModuleTagsDocumentFilter : IDocumentFilter
{
    private readonly Dictionary<string, string> _moduleDescriptions = new()
    {
        ["Users"] = "Gerenciamento de usuários, perfis e autenticação",
        //["Services"] = "Catálogo de serviços e categorias",
        //["Bookings"] = "Agendamentos e execução de serviços",
        //["Notifications"] = "Sistema de notificações e comunicação",
        //["Reports"] = "Relatórios e analytics do sistema",
        //["Admin"] = "Funcionalidades administrativas do sistema",
        ["Health"] = "Monitoramento e health checks dos serviços"
    };

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Organizar tags em ordem lógica
        var orderedTags = new List<string> { "Users",/* "Services", "Bookings", "Notifications", "Reports", "Admin",*/ "Health" };

        // Criar tags com descrições
        swaggerDoc.Tags = [];

        foreach (var tagName in orderedTags)
        {
            if (_moduleDescriptions.TryGetValue(tagName, out var description))
            {
                swaggerDoc.Tags.Add(new OpenApiTag
                {
                    Name = tagName,
                    Description = description
                });
            }
        }

        // Adicionar tags que não estão na lista pré-definida
        var usedTags = GetUsedTagsFromPaths(swaggerDoc);
        foreach (var tag in usedTags.Where(t => !orderedTags.Contains(t)))
        {
            swaggerDoc.Tags.Add(new OpenApiTag
            {
                Name = tag,
                Description = $"Operações relacionadas a {tag}"
            });
        }

        // Adicionar informações de servidor
        AddServerInformation(swaggerDoc);

        // Adicionar exemplos globais
        AddGlobalExamples(swaggerDoc);
    }

    private static HashSet<string> GetUsedTagsFromPaths(OpenApiDocument swaggerDoc)
    {
        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Guard against null Paths collection
        if (swaggerDoc.Paths == null)
            return tags;

        foreach (var path in swaggerDoc.Paths.Values)
        {
            // Guard against null path
            if (path?.Operations == null)
                continue;

            foreach (var operation in path.Operations.Values)
            {
                // Guard against null operation or Tags collection
                if (operation?.Tags == null)
                    continue;

                foreach (var tag in operation.Tags)
                {
                    // Skip tags with null or empty Name
                    if (!string.IsNullOrEmpty(tag?.Name))
                    {
                        tags.Add(tag.Name);
                    }
                }
            }
        }

        return tags;
    }

    private static void AddServerInformation(OpenApiDocument swaggerDoc)
    {
        swaggerDoc.Servers =
        [
            new OpenApiServer
            {
                Url = "http://localhost:5000",
                Description = "Desenvolvimento Local"
            },
            new OpenApiServer
            {
                Url = "https://api.meajudaai.com",
                Description = "Produção"
            }
        ];
    }

    private static void AddGlobalExamples(OpenApiDocument swaggerDoc)
    {
        // Adicionar componentes reutilizáveis
        swaggerDoc.Components ??= new OpenApiComponents();

        // Exemplo de erro padrão
        swaggerDoc.Components.Examples ??= new Dictionary<string, OpenApiExample>();

        swaggerDoc.Components.Examples["ErrorResponse"] = new OpenApiExample
        {
            Summary = "Resposta de Erro Padrão",
            Description = "Formato padrão das respostas de erro da API",
            Value = new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["type"] = new Microsoft.OpenApi.Any.OpenApiString("ValidationError"),
                ["title"] = new Microsoft.OpenApi.Any.OpenApiString("Dados de entrada inválidos"),
                ["status"] = new Microsoft.OpenApi.Any.OpenApiInteger(400),
                ["detail"] = new Microsoft.OpenApi.Any.OpenApiString("Um ou mais campos contêm valores inválidos"),
                ["instance"] = new Microsoft.OpenApi.Any.OpenApiString("/api/v1/users"),
                ["errors"] = new Microsoft.OpenApi.Any.OpenApiObject
                {
                    ["email"] = new Microsoft.OpenApi.Any.OpenApiArray
                    {
                        new Microsoft.OpenApi.Any.OpenApiString("O campo Email é obrigatório"),
                        new Microsoft.OpenApi.Any.OpenApiString("Email deve ter um formato válido")
                    }
                },
                ["traceId"] = new Microsoft.OpenApi.Any.OpenApiString("0HN7GKZB8K9QA:00000001")
            }
        };

        swaggerDoc.Components.Examples["SuccessResponse"] = new OpenApiExample
        {
            Summary = "Resposta de Sucesso Padrão",
            Description = "Formato padrão das respostas de sucesso da API",
            Value = new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["success"] = new Microsoft.OpenApi.Any.OpenApiBoolean(true),
                ["data"] = new Microsoft.OpenApi.Any.OpenApiObject
                {
                    ["id"] = new Microsoft.OpenApi.Any.OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    ["createdAt"] = new Microsoft.OpenApi.Any.OpenApiString("2024-01-15T10:30:00Z")
                },
                ["metadata"] = new Microsoft.OpenApi.Any.OpenApiObject
                {
                    ["requestId"] = new Microsoft.OpenApi.Any.OpenApiString("req_abc123"),
                    ["version"] = new Microsoft.OpenApi.Any.OpenApiString("1.0"),
                    ["timestamp"] = new Microsoft.OpenApi.Any.OpenApiString("2024-01-15T10:30:00Z")
                }
            }
        };

        // Schemas reutilizáveis
        swaggerDoc.Components.Schemas ??= new Dictionary<string, OpenApiSchema>();

        swaggerDoc.Components.Schemas["PaginationMetadata"] = new OpenApiSchema
        {
            Type = "object",
            Description = "Metadados de paginação para listagens",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["page"] = new OpenApiSchema { Type = "integer", Description = "Página atual (base 1)", Example = new Microsoft.OpenApi.Any.OpenApiInteger(1) },
                ["pageSize"] = new OpenApiSchema { Type = "integer", Description = "Itens por página", Example = new Microsoft.OpenApi.Any.OpenApiInteger(20) },
                ["totalItems"] = new OpenApiSchema { Type = "integer", Description = "Total de itens", Example = new Microsoft.OpenApi.Any.OpenApiInteger(150) },
                ["totalPages"] = new OpenApiSchema { Type = "integer", Description = "Total de páginas", Example = new Microsoft.OpenApi.Any.OpenApiInteger(8) },
                ["hasNextPage"] = new OpenApiSchema { Type = "boolean", Description = "Indica se há próxima página", Example = new Microsoft.OpenApi.Any.OpenApiBoolean(true) },
                ["hasPreviousPage"] = new OpenApiSchema { Type = "boolean", Description = "Indica se há página anterior", Example = new Microsoft.OpenApi.Any.OpenApiBoolean(false) }
            },
            Required = new HashSet<string> { "page", "pageSize", "totalItems", "totalPages", "hasNextPage", "hasPreviousPage" }
        };
    }
}
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MeAjudaAi.ApiService.Filters;

/// <summary>
/// Filtro para adicionar exemplos automáticos aos schemas baseado em atributos.
/// TODO: Reativar após migração para Swashbuckle 10.x. OpenApiSchema.Example é read-only,
///       precisa usar reflexão para acessar propriedade na implementação concreta.
///       Rastrear em: https://github.com/frigini/MeAjudaAi/issues/TBD
/// </summary>
public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Swashbuckle 10.x: OpenApiSchema.Example é read-only
        // SOLUÇÃO: Usar reflexão para acessar implementação concreta quando reativado
        throw new NotImplementedException("Requer migração para Swashbuckle 10.x - usar reflexão para Example property");
    }
}

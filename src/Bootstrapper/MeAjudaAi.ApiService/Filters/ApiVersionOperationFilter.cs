using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MeAjudaAi.ApiService.Filters;

public class ApiVersionOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiVersionParameter = operation.Parameters?.FirstOrDefault(p => p.Name == "version");
        if (apiVersionParameter != null)
        {
            operation.Parameters?.Remove(apiVersionParameter);
        }
    }
}
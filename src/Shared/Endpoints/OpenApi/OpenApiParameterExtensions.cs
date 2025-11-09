using Microsoft.OpenApi.Models;

namespace MeAjudaAi.Shared.Endpoints.OpenApi;

/// <summary>
/// Extensões para configuração padronizada de parâmetros OpenAPI nos endpoints.
/// </summary>
/// <remarks>
/// Centraliza a criação de parâmetros OpenAPI recorrentes como paginação,
/// filtros de busca e outros parâmetros comuns em endpoints REST.
/// </remarks>
public static class OpenApiParameterExtensions
{
    /// <summary>
    /// Adiciona parâmetro de termo de busca (searchTerm) para endpoints de consulta.
    /// </summary>
    /// <param name="operation">Operação OpenAPI</param>
    /// <param name="description">Descrição personalizada do parâmetro</param>
    /// <param name="example">Exemplo de valor</param>
    /// <returns>A mesma operação para fluent API</returns>
    public static OpenApiOperation AddSearchTermParameter(
        this OpenApiOperation operation,
        string description = "Termo de busca para filtrar resultados",
        string example = "joão")
    {
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "searchTerm",
            In = ParameterLocation.Query,
            Description = description,
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Example = new Microsoft.OpenApi.Any.OpenApiString(example)
            }
        });
        return operation;
    }

    /// <summary>
    /// Adiciona parâmetro de filtro por nome para endpoints de consulta.
    /// </summary>
    /// <param name="operation">Operação OpenAPI</param>
    /// <param name="description">Descrição personalizada do parâmetro</param>
    /// <param name="example">Exemplo de valor</param>
    /// <returns>A mesma operação para fluent API</returns>
    public static OpenApiOperation AddNameFilterParameter(
        this OpenApiOperation operation,
        string description = "Filtro por nome",
        string example = "joão")
    {
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "name",
            In = ParameterLocation.Query,
            Description = description,
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Example = new Microsoft.OpenApi.Any.OpenApiString(example)
            }
        });
        return operation;
    }

    /// <summary>
    /// Adiciona parâmetros padrão de paginação (pageNumber e pageSize).
    /// </summary>
    /// <param name="operation">Operação OpenAPI</param>
    /// <param name="defaultPageSize">Tamanho padrão da página</param>
    /// <param name="maxPageSize">Tamanho máximo da página</param>
    /// <returns>A mesma operação para fluent API</returns>
    public static OpenApiOperation AddPaginationParameters(
        this OpenApiOperation operation,
        int defaultPageSize = 10,
        int maxPageSize = 100)
    {
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "pageNumber",
            In = ParameterLocation.Query,
            Description = "Número da página (base 1)",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "integer",
                Minimum = 1,
                Default = new Microsoft.OpenApi.Any.OpenApiInteger(1),
                Example = new Microsoft.OpenApi.Any.OpenApiInteger(1)
            }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "pageSize",
            In = ParameterLocation.Query,
            Description = "Quantidade de itens por página",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "integer",
                Minimum = 1,
                Maximum = maxPageSize,
                Default = new Microsoft.OpenApi.Any.OpenApiInteger(defaultPageSize),
                Example = new Microsoft.OpenApi.Any.OpenApiInteger(defaultPageSize)
            }
        });

        return operation;
    }

    /// <summary>
    /// Adiciona parâmetro de filtro por tipo (genérico para IDs numéricos).
    /// </summary>
    /// <param name="operation">Operação OpenAPI</param>
    /// <param name="name">Nome do parâmetro</param>
    /// <param name="description">Descrição do parâmetro</param>
    /// <param name="example">Exemplo de valor</param>
    /// <param name="minimumValue">Valor mínimo permitido</param>
    /// <returns>A mesma operação para fluent API</returns>
    public static OpenApiOperation AddTypeFilterParameter(
        this OpenApiOperation operation,
        string name = "type",
        string description = "Filtro por tipo (ID numérico)",
        int example = 1,
        int minimumValue = 1)
    {
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = name,
            In = ParameterLocation.Query,
            Description = description,
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "integer",
                Minimum = minimumValue,
                Example = new Microsoft.OpenApi.Any.OpenApiInteger(example)
            }
        });
        return operation;
    }

    /// <summary>
    /// Adiciona parâmetro de filtro por status de verificação.
    /// </summary>
    /// <param name="operation">Operação OpenAPI</param>
    /// <param name="description">Descrição personalizada do parâmetro</param>
    /// <param name="example">Exemplo de valor</param>
    /// <returns>A mesma operação para fluent API</returns>
    public static OpenApiOperation AddVerificationStatusParameter(
        this OpenApiOperation operation,
        string description = "Status de verificação (ID numérico)",
        int example = 2)
    {
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "verificationStatus",
            In = ParameterLocation.Query,
            Description = description,
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "integer",
                Minimum = 0,
                Example = new Microsoft.OpenApi.Any.OpenApiInteger(example)
            }
        });
        return operation;
    }

    /// <summary>
    /// Adiciona parâmetro de filtro por ID GUID.
    /// </summary>
    /// <param name="operation">Operação OpenAPI</param>
    /// <param name="name">Nome do parâmetro</param>
    /// <param name="description">Descrição do parâmetro</param>
    /// <param name="example">Exemplo de valor GUID</param>
    /// <returns>A mesma operação para fluent API</returns>
    public static OpenApiOperation AddGuidParameter(
        this OpenApiOperation operation,
        string name,
        string description,
        string example = "123e4567-e89b-12d3-a456-426614174000")
    {
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = name,
            In = ParameterLocation.Query,
            Description = description,
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Format = "uuid",
                Example = new Microsoft.OpenApi.Any.OpenApiString(example)
            }
        });
        return operation;
    }

    /// <summary>
    /// Adiciona parâmetro de filtro por email.
    /// </summary>
    /// <param name="operation">Operação OpenAPI</param>
    /// <param name="description">Descrição personalizada do parâmetro</param>
    /// <param name="example">Exemplo de valor</param>
    /// <returns>A mesma operação para fluent API</returns>
    public static OpenApiOperation AddEmailFilterParameter(
        this OpenApiOperation operation,
        string description = "Filtro por email",
        string example = "usuario@example.com")
    {
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "email",
            In = ParameterLocation.Query,
            Description = description,
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Format = "email",
                Example = new Microsoft.OpenApi.Any.OpenApiString(example)
            }
        });
        return operation;
    }

    /// <summary>
    /// Adiciona parâmetro de filtro por cidade.
    /// </summary>
    /// <param name="operation">Operação OpenAPI</param>
    /// <param name="description">Descrição personalizada do parâmetro</param>
    /// <param name="example">Exemplo de valor</param>
    /// <returns>A mesma operação para fluent API</returns>
    public static OpenApiOperation AddCityFilterParameter(
        this OpenApiOperation operation,
        string description = "Filtro por cidade",
        string example = "São Paulo")
    {
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "city",
            In = ParameterLocation.Path,
            Description = description,
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Example = new Microsoft.OpenApi.Any.OpenApiString(example)
            }
        });
        return operation;
    }

    /// <summary>
    /// Adiciona parâmetro de filtro por estado.
    /// </summary>
    /// <param name="operation">Operação OpenAPI</param>
    /// <param name="description">Descrição personalizada do parâmetro</param>
    /// <param name="example">Exemplo de valor</param>
    /// <returns>A mesma operação para fluent API</returns>
    public static OpenApiOperation AddStateFilterParameter(
        this OpenApiOperation operation,
        string description = "Filtro por estado",
        string example = "SP")
    {
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "state",
            In = ParameterLocation.Path,
            Description = description,
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Example = new Microsoft.OpenApi.Any.OpenApiString(example)
            }
        });
        return operation;
    }
}

using Asp.Versioning;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Shared.Endpoints;

public abstract class BaseEndpoint
{
    /// <summary>
    /// Cria um grupo versionado usando apenas segmentos de URL com Asp.Versioning unificado
    /// Padrão: /api/v{version:apiVersion}/{module} (exemplo: /api/v1/users)
    /// Esta abordagem é explícita, clara e evita a complexidade de múltiplos métodos de versionamento
    /// </summary>
    /// <param name="app">Construtor de rotas de endpoint</param>
    /// <param name="module">Nome do módulo (ex: "users", "services")</param>
    /// <param name="tag">Tag do OpenAPI (padrão é o nome do módulo)</param>
    /// <returns>Route group builder configurado para registro de endpoints</returns>
    public static RouteGroupBuilder CreateVersionedGroup(
        IEndpointRouteBuilder app,
        string module,
        string? tag = null)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        // Usa apenas o padrão de segmento de URL: /api/v1/users
        // Esta é a abordagem de versionamento mais explícita e clara
        return app.MapGroup($"/api/v{{version:apiVersion}}/{module}")
            .WithApiVersionSet(versionSet)
            .WithTags(tag ?? char.ToUpper(module[0]) + module[1..])
            .WithOpenApi();
    }



    /// <summary>
    /// Manipula qualquer Result&lt;T&gt; automaticamente. Suporta respostas Ok e Created.
    /// </summary>
    /// <param name="result">O resultado a ser manipulado</param>
    /// <param name="createdRoute">Nome da rota opcional para resposta Created</param>
    /// <param name="routeValues">Valores de rota opcionais para resposta Created</param>
    protected static IResult Handle<T>(Result<T> result, string? createdRoute = null, object? routeValues = null)
        => EndpointExtensions.Handle(result, createdRoute, routeValues);

    /// <summary>
    /// Manipula Result não genérico automaticamente
    /// </summary>
    protected static IResult Handle(Result result)
        => EndpointExtensions.Handle(result);

    /// <summary>
    /// Manipula resultados paginados automaticamente
    /// </summary>
    protected static IResult HandlePaged<T>(Result<IEnumerable<T>> result, int total, int page, int size)
        => EndpointExtensions.HandlePaged(result, total, page, size);

    /// <summary>
    /// Manipula PagedResult diretamente - sem necessidade de extração manual
    /// </summary>
    protected static IResult HandlePagedResult<T>(Result<PagedResult<T>> result)
        => EndpointExtensions.HandlePagedResult(result);

    /// <summary>
    /// Manipula resultados que devem retornar NoContent em caso de sucesso
    /// </summary>
    protected static IResult HandleNoContent<T>(Result<T> result)
        => EndpointExtensions.HandleNoContent(result);

    /// <summary>
    /// Manipula resultados que devem retornar NoContent em caso de sucesso (não genérico)
    /// </summary>
    protected static IResult HandleNoContent(Result result)
        => EndpointExtensions.HandleNoContent(result);

    /// <summary>
    /// Resposta BadRequest direta (para cenários sem Result)
    /// </summary>
    protected static IResult BadRequest(string message) =>
        TypedResults.BadRequest(new Response<object>(null, 400, message));

    /// <summary>
    /// Resposta BadRequest direta usando objeto Error
    /// </summary>
    protected static IResult BadRequest(Error error) =>
        TypedResults.BadRequest(new Response<object>(null, error.StatusCode, error.Message));

    /// <summary>
    /// Resposta NotFound direta (para cenários sem Result)
    /// </summary>
    protected static IResult NotFound(string message) =>
        TypedResults.NotFound(new Response<object>(null, 404, message));

    /// <summary>
    /// Resposta NotFound direta usando objeto Error
    /// </summary>
    protected static IResult NotFound(Error error) =>
        TypedResults.NotFound(new Response<object>(null, error.StatusCode, error.Message));

    /// <summary>
    /// Resposta Unauthorized direta
    /// </summary>
    protected static IResult Unauthorized() => TypedResults.Unauthorized();

    /// <summary>
    /// Resposta Forbidden direta
    /// </summary>
    protected static IResult Forbid() => TypedResults.Forbid();

    protected static string GetUserId(HttpContext context)
    {
        var userId = context.User?.FindFirst("sub")?.Value ??
                     context.User?.FindFirst("id")?.Value;

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User ID not found in token");

        return userId;
    }

    protected static string? GetUserIdOrNull(HttpContext context)
    {
        return context.User?.FindFirst("sub")?.Value ??
               context.User?.FindFirst("id")?.Value;
    }
}

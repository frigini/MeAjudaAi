using Asp.Versioning;
using Asp.Versioning.Builder;
using MeAjudaAi.Shared.Common;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Shared.Endpoints;

public abstract class BaseEndpoint
{
    /// <summary>
    /// Creates a versioned group using unified Asp.Versioning with URL segments only
    /// Pattern: /api/v{version:apiVersion}/{module} (e.g., /api/v1/users)
    /// This approach is explicit, clear, and avoids complexity of multiple versioning methods
    /// </summary>
    /// <param name="app">Endpoint route builder</param>
    /// <param name="module">Module name (e.g., "users", "services")</param>
    /// <param name="tag">OpenAPI tag (defaults to module name)</param>
    /// <returns>Configured route group builder for endpoint registration</returns>
    public static RouteGroupBuilder CreateVersionedGroup(
        IEndpointRouteBuilder app,
        string module,
        string? tag = null)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        // Use URL segment pattern only: /api/v1/users
        // This is the most explicit and clear versioning approach
        return app.MapGroup($"/api/v{{version:apiVersion}}/{module}")
            .WithApiVersionSet(versionSet)
            .WithTags(tag ?? char.ToUpper(module[0]) + module[1..])
            .WithOpenApi();
    }

    /// <summary>
    /// Creates a legacy versioned group (for backward compatibility)
    /// </summary>
    [Obsolete("Use CreateVersionedGroup(app, module, tag) instead")]
    protected static RouteGroupBuilder CreateGroup(
        IEndpointRouteBuilder app,
        string prefix,
        string tag,
        int majorVersion = 1,
        int minorVersion = 0)
    {
        var version = new ApiVersion(majorVersion, minorVersion);
        var apiVersionSet = app.NewApiVersionSet()
                       .HasApiVersion(version)
                       .Build();

        return app.MapGroup($"/api/v{majorVersion}/{prefix}")
                .WithTags(tag)
                .WithApiVersionSet(apiVersionSet)
                .WithOpenApi();
    }

    /// <summary>
    /// Creates a legacy versioned group with specific version (for backward compatibility)
    /// </summary>
    [Obsolete("Use CreateVersionedGroup(app, module, tag) instead")]

    protected static RouteGroupBuilder CreateVersionedGroup(
        IEndpointRouteBuilder app,
        string prefix,
        string tag,
        ApiVersion version,
        ApiVersionSet? versionSet = null)
    {
        var group = app.MapGroup($"/api/v{version.MajorVersion}/{prefix}")
                       .WithTags(tag);

        if (versionSet != null)
        {
            group = group.WithApiVersionSet(versionSet);
        }
        else
        {
            var defaultVersionSet = app.NewApiVersionSet()
                                       .HasApiVersion(version)
                                       .Build();
            group = group.WithApiVersionSet(defaultVersionSet);
        }

        return group.WithOpenApi();
    }

    /// <summary>
    /// Handle any Result&lt;T&gt; automatically. Supports Ok and Created responses.
    /// </summary>
    /// <param name="result">The result to handle</param>
    /// <param name="createdRoute">Optional route name for Created response</param>
    /// <param name="routeValues">Optional route values for Created response</param>
    protected static IResult Handle<T>(Result<T> result, string? createdRoute = null, object? routeValues = null)
        => EndpointExtensions.Handle(result, createdRoute, routeValues);

    /// <summary>
    /// Handle non-generic Result automatically
    /// </summary>
    protected static IResult Handle(Result result)
        => EndpointExtensions.Handle(result);

    /// <summary>
    /// Handle paged results automatically
    /// </summary>
    protected static IResult HandlePaged<T>(Result<IEnumerable<T>> result, int total, int page, int size)
        => EndpointExtensions.HandlePaged(result, total, page, size);

    /// <summary>
    /// Handle PagedResult directly - no manual extraction needed
    /// </summary>
    protected static IResult HandlePagedResult<T>(Result<PagedResult<T>> result)
        => EndpointExtensions.HandlePagedResult(result);

    /// <summary>
    /// Handle results that should return NoContent on success
    /// </summary>
    protected static IResult HandleNoContent<T>(Result<T> result)
        => EndpointExtensions.HandleNoContent(result);

    /// <summary>
    /// Handle results that should return NoContent on success (non-generic)
    /// </summary>
    protected static IResult HandleNoContent(Result result)
        => EndpointExtensions.HandleNoContent(result);

    /// <summary>
    /// Direct BadRequest response (for non-Result scenarios)
    /// </summary>
    protected static IResult BadRequest(string message) =>
        TypedResults.BadRequest(new Response<object>(null, 400, message));

    /// <summary>
    /// Direct BadRequest response using Error object
    /// </summary>
    protected static IResult BadRequest(Error error) =>
        TypedResults.BadRequest(new Response<object>(null, error.StatusCode, error.Message));

    /// <summary>
    /// Direct NotFound response (for non-Result scenarios)
    /// </summary>
    protected static IResult NotFound(string message) =>
        TypedResults.NotFound(new Response<object>(null, 404, message));

    /// <summary>
    /// Direct NotFound response using Error object
    /// </summary>
    protected static IResult NotFound(Error error) =>
        TypedResults.NotFound(new Response<object>(null, error.StatusCode, error.Message));

    /// <summary>
    /// Direct Unauthorized response
    /// </summary>
    protected static IResult Unauthorized() => TypedResults.Unauthorized();

    /// <summary>
    /// Direct Forbidden response
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
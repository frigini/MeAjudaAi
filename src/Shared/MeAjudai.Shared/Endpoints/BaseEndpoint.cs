using Asp.Versioning;
using Asp.Versioning.Builder;
using MeAjudaAi.Shared.Common;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Shared.Endpoints;

public abstract class BaseEndpoint
{
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

    // Métodos auxiliares para respostas
    protected static IResult Ok<T>(Result<T> result) => EndpointExtensions.HandleResult(result);
    protected static IResult Ok(Result result) => EndpointExtensions.HandleResult(result);

    protected static IResult Created<T>(Result<T> result, string routeName, object? routeValues = null) =>
        EndpointExtensions.HandleCreatedResult(result, routeName, routeValues);

    protected static IResult NoContent(Result result) => EndpointExtensions.HandleNoContentResult(result);
    protected static IResult NoContent<T>(Result<T> result) => EndpointExtensions.HandleNoContentResult(result);

    protected static IResult Paged<T>(Result<IEnumerable<T>> result, int total, int page, int size) =>
        EndpointExtensions.HandlePagedResult(result, total, page, size);

    // Métodos auxiliares diretos
    protected static IResult BadRequest(string message) =>
        TypedResults.BadRequest(new Response<object>(null, 400, message));

    protected static IResult NotFound(string message) =>
        TypedResults.NotFound(new Response<object>(null, 404, message));

    protected static IResult Unauthorized() => TypedResults.Unauthorized();
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
using MeAjudaAi.Shared.Common;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Shared.Endpoints;

public static class EndpointExtensions
{
    /// <summary>
    /// Universal method to handle any Result type and return appropriate HTTP response
    /// Supports Ok, Created, NotFound, BadRequest, and other error responses automatically
    /// </summary>
    public static IResult Handle<T>(Result<T> result, string? createdRoute = null, object? routeValues = null)
    {
        if (result.IsSuccess)
        {
            if (!string.IsNullOrEmpty(createdRoute))
            {
                var createdResponse = new Response<T>(result.Value, 201, "Criado com sucesso");
                return TypedResults.CreatedAtRoute(createdResponse, createdRoute, routeValues);
            }
            
            return TypedResults.Ok(new Response<T>(result.Value));
        }

        return CreateErrorResponse<T>(result.Error);
    }

    /// <summary>
    /// Handle Result (non-generic) with automatic response determination
    /// </summary>
    public static IResult Handle(Result result)
    {
        if (result.IsSuccess)
            return TypedResults.Ok(new Response<object>(null));

        return CreateErrorResponse<object>(result.Error);
    }

    /// <summary>
    /// Handle paged results with automatic response formatting
    /// </summary>
    public static IResult HandlePaged<T>(Result<IEnumerable<T>> result, int totalCount, int currentPage, int pageSize)
    {
        if (result.IsSuccess)
        {
            var pagedResponse = new PagedResponse<IEnumerable<T>>(
                result.Value,
                currentPage,
                pageSize,
                totalCount);

            return TypedResults.Ok(pagedResponse);
        }

        return CreateErrorResponse<IEnumerable<T>>(result.Error);
    }

    /// <summary>
    /// Handle PagedResult directly - extracts pagination info automatically
    /// </summary>
    public static IResult HandlePagedResult<T>(Result<PagedResult<T>> result)
    {
        if (result.IsSuccess)
        {
            var pagedData = result.Value;
            var pagedResponse = new PagedResponse<IEnumerable<T>>(
                pagedData.Items,
                pagedData.Page,
                pagedData.PageSize,
                pagedData.TotalCount);

            return TypedResults.Ok(pagedResponse);
        }

        return CreateErrorResponse<PagedResult<T>>(result.Error);
    }

    /// <summary>
    /// Handle results that should return NoContent on success
    /// </summary>
    public static IResult HandleNoContent<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return TypedResults.NoContent();

        return CreateErrorResponse<T>(result.Error);
    }

    /// <summary>
    /// Handle results that should return NoContent on success (non-generic)
    /// </summary>
    public static IResult HandleNoContent(Result result)
    {
        if (result.IsSuccess)
            return TypedResults.NoContent();

        return CreateErrorResponse<object>(result.Error);
    }

    private static IResult CreateErrorResponse<T>(Error error)
    {
        var response = new Response<T>(default, error.StatusCode, error.Message);

        return error.StatusCode switch
        {
            404 => TypedResults.NotFound(response),
            400 => TypedResults.BadRequest(response),
            401 => TypedResults.Unauthorized(),
            403 => TypedResults.Forbid(),
            500 => TypedResults.Problem(
                detail: error.Message,
                statusCode: 500,
                title: "Internal Server Error"),
            _ => TypedResults.BadRequest(response)
        };
    }
}
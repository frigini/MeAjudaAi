using MeAjudaAi.Shared.Common;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Shared.Endpoints;

public static class EndpointExtensions
{
    public static IResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return TypedResults.Ok(new Response<T>(result.Value));

        return CreateErrorResponse<T>(result.Error);
    }

    public static IResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return TypedResults.Ok(new Response<object>(null));

        return CreateErrorResponse<object>(result.Error);
    }

    public static IResult HandlePagedResult<T>(Result<IEnumerable<T>> result, int totalCount, int currentPage, int pageSize)
    {
        if (result.IsSuccess)
        {
            var pagedResponse = new PagedResponse<IEnumerable<T>>(
                result.Value,
                totalCount,
                currentPage,
                pageSize);

            return TypedResults.Ok(pagedResponse);
        }

        return CreateErrorResponse<IEnumerable<T>>(result.Error);
    }

    public static IResult HandleNoContentResult(Result result)
    {
        if (result.IsSuccess)
            return TypedResults.NoContent();

        return CreateErrorResponse<object>(result.Error);
    }

    public static IResult HandleNoContentResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return TypedResults.NoContent();

        return CreateErrorResponse<T>(result.Error);
    }

    public static IResult HandleCreatedResult<T>(
        Result<T> result,
        string routeName,
        object? routeValues = null)
    {
        if (result.IsSuccess)
        {
            var response = new Response<T>(result.Value, 201, "Criado com sucesso");
            return TypedResults.CreatedAtRoute(response, routeName, routeValues);
        }

        return CreateErrorResponse<T>(result.Error);
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
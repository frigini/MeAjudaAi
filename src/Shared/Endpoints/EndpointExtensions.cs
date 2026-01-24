using MeAjudaAi.Contracts;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;

using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Shared.Endpoints;

public static class EndpointExtensions
{
    /// <summary>
    /// Método universal para manipular qualquer tipo Result e retornar a resposta HTTP apropriada
    /// Suporta Ok, Created, NotFound, BadRequest e outras respostas de erro automaticamente
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
    /// Manipula Result (não genérico) com determinação automática da resposta
    /// </summary>
    public static IResult Handle(Result result)
    {
        if (result.IsSuccess)
            return TypedResults.Ok(new Response<object>(null));

        return CreateErrorResponse<object>(result.Error);
    }

    /// <summary>
    /// Manipula resultados paginados com formatação automática da resposta
    /// </summary>
    public static IResult HandlePaged<T>(Result<IEnumerable<T>> result, int totalCount, int currentPage, int pageSize)
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

    /// <summary>
    /// Manipula PagedResult diretamente - extrai informações de paginação automaticamente
    /// </summary>
    public static IResult HandlePagedResult<T>(Result<PagedResult<T>> result)
    {
        if (result.IsSuccess)
        {
            var pagedData = result.Value;
            var pagedResponse = new PagedResponse<IEnumerable<T>>(
                pagedData.Items,          // dados
                pagedData.TotalItems,     // totalCount
                pagedData.PageNumber,     // página atual
                pagedData.PageSize        // tamanho da página
            );

            return TypedResults.Ok(pagedResponse);
        }

        return CreateErrorResponse<PagedResult<T>>(result.Error);
    }

    /// <summary>
    /// Manipula resultados que devem retornar NoContent em caso de sucesso
    /// </summary>
    public static IResult HandleNoContent<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return TypedResults.NoContent();

        return CreateErrorResponse<T>(result.Error);
    }

    /// <summary>
    /// Manipula resultados que devem retornar NoContent em caso de sucesso (não genérico)
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

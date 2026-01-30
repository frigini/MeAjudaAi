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
                // Para Created, ainda precisamos retornar o Result<T> completo para manter o contrato
                return TypedResults.CreatedAtRoute(result, createdRoute, routeValues);
            }

            // CORREÇÃO CRÍTICA: Retorna o Result<T> completo, não apenas o Value
            // O cliente espera { "isSuccess": true, "value": { ... }, "error": null }
            return TypedResults.Ok(result);
        }

        return CreateErrorResponse<T>(result.Error);
    }

    /// <summary>
    /// Manipula Result (não genérico) com determinação automática da resposta
    /// </summary>
    public static IResult Handle(Result result)
    {
        if (result.IsSuccess)
        {
            // Retorna o Result completo para vazio também
            return TypedResults.Ok(result);
        }

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
            
            // Retorna Result<PagedResponse> para manter consistência
            // Usa conversão implícita ou construtor explícito já que Result.Success(val) não existe na classe não-genérica
            Result<PagedResponse<IEnumerable<T>>> wrappedResult = pagedResponse;
            return TypedResults.Ok(wrappedResult);
        }

        return CreateErrorResponse<IEnumerable<T>>(result.Error);
    }

    /// <summary>
    /// Manipula PagedResult diretamente - extrai informações de paginação automaticamente
    /// </summary>
    public static IResult HandlePagedResult<T>(Result<PagedResult<T>> result)
    {
        // Se o result já é um Result<PagedResult<T>>, retornamos ele diretamente
        // O cliente espera Result<PagedResult<T>>
        
        if (result.IsSuccess)
        {
            return TypedResults.Ok(result);
        }

        return CreateErrorResponse<PagedResult<T>>(result.Error);
    }

    /// <summary>
    /// Manipula resultados que devem retornar NoContent em caso de sucesso
    /// </summary>
    public static IResult HandleNoContent<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            // CORREÇÃO CRÍTICA: Retornar 200 OK com o Result<T> completo
            // O frontend espera { "isSuccess": true, "value": ... }
            // 204 No Content retorna corpo vazio, quebrando a deserialização do Result<T> no cliente Refit.
            return TypedResults.Ok(result);
        }

        return CreateErrorResponse<T>(result.Error);
    }

    /// <summary>
    /// Manipula resultados que devem retornar NoContent em caso de sucesso (não genérico)
    /// </summary>
    public static IResult HandleNoContent(Result result)
    {
        if (result.IsSuccess)
        {
            // CORREÇÃO CRÍTICA: Retornar 200 OK com o Result completo
            return TypedResults.Ok(result);
        }

        return CreateErrorResponse<object>(result.Error);
    }

    private static IResult CreateErrorResponse<T>(Error error)
    {
        // Fix: Retornar Result<T> (Falha) para consistência com o caminho de sucesso
        // O corpo da resposta de erro será { "isSuccess": false, "error": { ... }, "value": null }
        var failedResult = Result<T>.Failure(error);

        return error.StatusCode switch
        {
            404 => TypedResults.NotFound(failedResult),
            400 => TypedResults.BadRequest(failedResult),
            401 => TypedResults.Unauthorized(),
            403 => TypedResults.Forbid(),
            500 => TypedResults.Problem(
                detail: error.Message,
                statusCode: 500,
                title: "Erro Interno do Servidor"),
            _ => TypedResults.BadRequest(failedResult)
        };
    }
}

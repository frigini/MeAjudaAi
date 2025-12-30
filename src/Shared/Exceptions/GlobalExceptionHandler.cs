using MeAjudaAi.Shared.Database.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Exceptions;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Desencapsula exceções conhecidas de InvalidOperationException
        if (exception is InvalidOperationException { InnerException: { } inner } &&
            inner is ValidationException or NotFoundException or BadRequestException or UnprocessableEntityException)
        {
            exception = inner;
        }
        
        var (statusCode, title, detail, errors, extensions) = exception switch
        {
            // Nossa ValidationException customizada (400 - erros de formato/estrutura)
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Erro de validação",
                "Um ou mais erros de validação ocorreram",
                validationException.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()),
                new Dictionary<string, object?>()),

            // UnprocessableEntityException (422 - erros semânticos/regras de negócio)
            UnprocessableEntityException unprocessableException => (
                StatusCodes.Status422UnprocessableEntity,
                "Entidade Não Processável",
                unprocessableException.Message,
                null,
                CreateExtensionsWithNonNullValues(
                    ("entityName", unprocessableException.EntityName),
                    ("details", unprocessableException.Details))),

            UniqueConstraintException uniqueException => (
                StatusCodes.Status409Conflict,
                "Valor Duplicado",
                $"O valor para {uniqueException.ColumnName ?? "este campo"} já existe",
                null,
                CreateExtensionsWithNonNullValues(
                    ("constraintName", uniqueException.ConstraintName),
                    ("columnName", uniqueException.ColumnName))),

            NotNullConstraintException notNullException => (
                StatusCodes.Status400BadRequest,
                "Campo Obrigatório Ausente",
                $"O campo {notNullException.ColumnName ?? "este campo"} é obrigatório",
                null,
                CreateExtensionsWithNonNullValues(
                    ("columnName", notNullException.ColumnName))),

            ForeignKeyConstraintException foreignKeyException => (
                StatusCodes.Status400BadRequest,
                "Referência Inválida",
                "O registro referenciado não existe",
                null,
                CreateExtensionsWithNonNullValues(
                    ("constraintName", foreignKeyException.ConstraintName),
                    ("tableName", foreignKeyException.TableName))),

            DbUpdateException dbUpdateException => ProcessDbUpdateException(dbUpdateException),

            NotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                "Recurso Não Encontrado",
                notFoundException.Message,
                null,
                CreateExtensionsWithNonNullValues(
                    ("entityName", notFoundException.EntityName),
                    ("entityId", notFoundException.EntityId))),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Não Autorizado",
                "Autenticação é necessária para acessar este recurso",
                null,
                []),

            ForbiddenAccessException forbiddenException => (
                StatusCodes.Status403Forbidden,
                "Acesso Negado",
                forbiddenException.Message,
                null,
                []),

            BusinessRuleException businessException => (
                StatusCodes.Status400BadRequest,
                "Violação de Regra de Negócio",
                businessException.Message,
                null,
                CreateExtensionsWithNonNullValues(
                    ("ruleName", businessException.RuleName))),

            ArgumentException argumentException => (
                StatusCodes.Status400BadRequest,
                "Erro de validação",
                argumentException.Message,
                null,
                CreateExtensionsWithNonNullValues(
                    ("parameterName", argumentException.ParamName))),

            DomainException domainException => (
                StatusCodes.Status400BadRequest,
                "Violação de Regra de Domínio",
                domainException.Message,
                null,
                []),

            BadRequestException badRequestException => (
                StatusCodes.Status400BadRequest,
                "Erro de validação",
                badRequestException.Message,
                null,
                []),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro Interno do Servidor",
                "Ocorreu um erro inesperado ao processar sua requisição",
                null,
                new Dictionary<string, object?>
                {
                    ["traceId"] = httpContext.TraceIdentifier
                })
        };

        // Log com diferentes níveis baseado no tipo de erro
        if (statusCode >= 500)
        {
            logger.LogError(exception, "Server error occurred: {ErrorType} - Original Exception: {ExceptionDetails}",
                exception.GetType().Name, exception.ToString());
        }
        else if (statusCode >= 400)
        {
            logger.LogWarning("Client error occurred: {ErrorType} - {Message}", exception.GetType().Name, exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
            Type = GetProblemTypeUri(statusCode)
        };

        // Adicionar erros de validação
        if (errors != null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        // Adicionar extensões específicas
        foreach (var (key, value) in extensions)
        {
            problemDetails.Extensions[key] = value;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int statusCode, string title, string detail, object? errors, Dictionary<string, object?> extensions) ProcessDbUpdateException(DbUpdateException dbUpdateException)
    {
        // Tenta processar a exceção usando nosso processador customizado
        var processedException = PostgreSqlExceptionProcessor.ProcessException(dbUpdateException);

        if (processedException is UniqueConstraintException uniqueException)
        {
            return (
                StatusCodes.Status409Conflict,
                "Valor Duplicado",
                $"O valor para {uniqueException.ColumnName ?? "este campo"} já existe",
                null,
                CreateExtensionsWithNonNullValues(
                    ("constraintName", uniqueException.ConstraintName),
                    ("columnName", uniqueException.ColumnName)));
        }

        if (processedException is NotNullConstraintException notNullException)
        {
            return (
                StatusCodes.Status400BadRequest,
                "Campo Obrigatório Ausente",
                $"O campo {notNullException.ColumnName ?? "este campo"} é obrigatório",
                null,
                CreateExtensionsWithNonNullValues(
                    ("columnName", notNullException.ColumnName)));
        }

        if (processedException is ForeignKeyConstraintException foreignKeyException)
        {
            return (
                StatusCodes.Status400BadRequest,
                "Referência Inválida",
                "O registro referenciado não existe",
                null,
                CreateExtensionsWithNonNullValues(
                    ("constraintName", foreignKeyException.ConstraintName),
                    ("tableName", foreignKeyException.TableName)));
        }

        // Fallback para DbUpdateException genérica
        return (
            StatusCodes.Status400BadRequest,
            "Erro de Banco de Dados",
            "Ocorreu um erro de banco de dados ao processar sua requisição",
            null,
            new Dictionary<string, object?>
            {
                ["exceptionType"] = dbUpdateException.GetType().Name
            });
    }

    private static string GetProblemTypeUri(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
        403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        422 => "https://tools.ietf.org/html/rfc4918#section-11.2",
        500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        _ => "https://tools.ietf.org/html/rfc7231"
    };

    /// <summary>
    /// Cria um dicionário de extensões excluindo valores nulos para respostas mais limpas.
    /// </summary>
    private static Dictionary<string, object?> CreateExtensionsWithNonNullValues(params (string key, object? value)[] entries)
    {
        return entries
            .Where(e => e.value != null)
            .ToDictionary(e => e.key, e => e.value);
    }
}

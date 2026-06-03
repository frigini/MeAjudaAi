using MeAjudaAi.Shared.Database.Exceptions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Exceptions;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Desencapsula exceções de wrappers comuns
        if ((exception is InvalidOperationException or System.Reflection.TargetInvocationException or AggregateException) 
            && exception.InnerException is { } inner
            && inner is ValidationException or BadRequestException or UnprocessableEntityException or ArgumentException or DomainException or NotFoundException)
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
                new Dictionary<string, object?> { ["traceId"] = httpContext.TraceIdentifier }),

            // UnprocessableEntityException (422 - erros semânticos/regras de negócio)
            UnprocessableEntityException unprocessableException => (
                StatusCodes.Status422UnprocessableEntity,
                "Entidade Não Processável",
                unprocessableException.Message,
                null,
                CreateExtensionsWithNonNullValues(
                    ("entityName", unprocessableException.EntityName),
                    ("details", unprocessableException.Details),
                    ("traceId", httpContext.TraceIdentifier))),

            UniqueConstraintException uniqueException => (
                StatusCodes.Status409Conflict,
                "Valor Duplicado",
                $"O valor para {uniqueException.ColumnName ?? "este campo"} já existe",
                null,
                CreateExtensionsWithNonNullValues(
                    ("constraintName", uniqueException.ConstraintName),
                    ("columnName", uniqueException.ColumnName),
                    ("traceId", httpContext.TraceIdentifier))),

            NotNullConstraintException notNullException => (
                StatusCodes.Status400BadRequest,
                "Campo Obrigatório Ausente",
                $"O campo {notNullException.ColumnName ?? "este campo"} é obrigatório",
                null,
                CreateExtensionsWithNonNullValues(
                    ("columnName", notNullException.ColumnName),
                    ("traceId", httpContext.TraceIdentifier))),

            ForeignKeyConstraintException foreignKeyException => (
                StatusCodes.Status400BadRequest,
                "Referência Inválida",
                "O registro referenciado não existe",
                null,
                CreateExtensionsWithNonNullValues(
                    ("constraintName", foreignKeyException.ConstraintName),
                    ("tableName", foreignKeyException.TableName),
                    ("traceId", httpContext.TraceIdentifier))),

            DbUpdateException dbUpdateException => ProcessDbUpdateException(dbUpdateException, httpContext.TraceIdentifier),

            NotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                "Recurso Não Encontrado",
                notFoundException.Message,
                null,
                CreateExtensionsWithNonNullValues(
                    ("entityName", notFoundException.EntityName),
                    ("entityId", notFoundException.EntityId),
                    ("traceId", httpContext.TraceIdentifier))),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Não Autorizado",
                "Autenticação é necessária para acessar este recurso",
                null,
                new Dictionary<string, object?> { ["traceId"] = httpContext.TraceIdentifier }),

            ForbiddenAccessException forbiddenException => (
                StatusCodes.Status403Forbidden,
                "Acesso Negado",
                forbiddenException.Message,
                null,
                new Dictionary<string, object?> { ["traceId"] = httpContext.TraceIdentifier }),

            BusinessRuleException businessException => (
                StatusCodes.Status400BadRequest,
                "Violação de Regra de Negócio",
                businessException.Message,
                null,
                CreateExtensionsWithNonNullValues(
                    ("ruleName", businessException.RuleName),
                    ("traceId", httpContext.TraceIdentifier))),

            ArgumentException argumentException => (
                StatusCodes.Status400BadRequest,
                "Erro de validação",
                argumentException.Message,
                null,
                CreateExtensionsWithNonNullValues(
                    ("parameterName", argumentException.ParamName),
                    ("traceId", httpContext.TraceIdentifier))),

            DomainException domainException => (
                StatusCodes.Status400BadRequest,
                "Violação de Regra de Domínio",
                domainException.Message,
                null,
                new Dictionary<string, object?> { ["traceId"] = httpContext.TraceIdentifier }),

            BadRequestException badRequestException => (
                StatusCodes.Status400BadRequest,
                "Erro de validação",
                badRequestException.Message,
                null,
                new Dictionary<string, object?> { ["traceId"] = httpContext.TraceIdentifier }),

            BadHttpRequestException badHttpRequestException => (
                badHttpRequestException.StatusCode is >= 400 and < 500 
                    ? badHttpRequestException.StatusCode 
                    : StatusCodes.Status400BadRequest,
                "Requisição inválida",
                "A requisição enviada é inválida ou está mal formatada.",
                null,
                CreateExtensionsWithNonNullValues(
                    ("originalMessage", env.IsDevelopment() ? badHttpRequestException.Message : null),
                    ("traceId", httpContext.TraceIdentifier))),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro Interno do Servidor",
                (env.IsEnvironment(EnvironmentNames.Testing) || env.IsDevelopment())
                    ? $"[{exception.GetType().Name}] {exception.Message} {exception.StackTrace}"
                    : "Ocorreu um erro inesperado",
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

        await httpContext.Response.WriteAsJsonAsync(problemDetails, options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);

        return true;
    }

    private static (int statusCode, string title, string detail, object? errors, Dictionary<string, object?> extensions) ProcessDbUpdateException(DbUpdateException dbUpdateException, string traceId)
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
                    ("columnName", uniqueException.ColumnName),
                    ("traceId", traceId)));
        }

        if (processedException is NotNullConstraintException notNullException)
        {
            return (
                StatusCodes.Status400BadRequest,
                "Campo Obrigatório Ausente",
                $"O campo {notNullException.ColumnName ?? "este campo"} é obrigatório",
                null,
                CreateExtensionsWithNonNullValues(
                    ("columnName", notNullException.ColumnName),
                    ("traceId", traceId)));
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
                    ("tableName", foreignKeyException.TableName),
                    ("traceId", traceId)));
        }

        // Fallback para DbUpdateException genérica
        return (
            StatusCodes.Status400BadRequest,
            "Erro de Banco de Dados",
            "Ocorreu um erro de banco de dados ao processar sua requisição",
            null,
            new Dictionary<string, object?>
            {
                ["exceptionType"] = dbUpdateException.GetType().Name,
                ["traceId"] = traceId
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

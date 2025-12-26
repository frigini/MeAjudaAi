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
        // Se InvalidOperationException tiver ValidationException como inner, desencapsular
        if (exception is InvalidOperationException && exception.InnerException is ValidationException innerValidationException)
        {
            exception = innerValidationException;
        }
        
        // Se InvalidOperationException tiver NotFoundException como inner, desencapsular
        if (exception is InvalidOperationException && exception.InnerException is NotFoundException innerNotFoundException)
        {
            exception = innerNotFoundException;
        }
        
        // Se InvalidOperationException tiver BadRequestException como inner, desencapsular
        if (exception is InvalidOperationException && exception.InnerException is BadRequestException innerBadRequestException)
        {
            exception = innerBadRequestException;
        }
        
        var (statusCode, title, detail, errors, extensions) = exception switch
        {
            // Nossa ValidationException customizada
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Erro de validação",
                "Um ou mais erros de validação ocorreram",
                validationException.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()),
                new Dictionary<string, object?>()),

            UniqueConstraintException uniqueException => (
                StatusCodes.Status409Conflict,
                "Valor Duplicado",
                $"O valor para {uniqueException.ColumnName ?? "este campo"} já existe",
                null,
                new Dictionary<string, object?>
                {
                    ["constraintName"] = uniqueException.ConstraintName,
                    ["columnName"] = uniqueException.ColumnName
                }),

            NotNullConstraintException notNullException => (
                StatusCodes.Status400BadRequest,
                "Required Field Missing",
                $"The field {notNullException.ColumnName ?? "this field"} is required",
                null,
                new Dictionary<string, object?>
                {
                    ["columnName"] = notNullException.ColumnName
                }),

            ForeignKeyConstraintException foreignKeyException => (
                StatusCodes.Status400BadRequest,
                "Invalid Reference",
                $"The referenced record does not exist",
                null,
                new Dictionary<string, object?>
                {
                    ["constraintName"] = foreignKeyException.ConstraintName,
                    ["tableName"] = foreignKeyException.TableName
                }),

            DbUpdateException dbUpdateException => ProcessDbUpdateException(dbUpdateException),

            NotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                notFoundException.Message,
                null,
                new Dictionary<string, object?>
                {
                    ["entityName"] = notFoundException.EntityName,
                    ["entityId"] = notFoundException.EntityId
                }),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "Authentication is required to access this resource",
                null,
                []),

            ForbiddenAccessException forbiddenException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                forbiddenException.Message,
                null,
                []),

            BusinessRuleException businessException => (
                StatusCodes.Status400BadRequest,
                "Business Rule Violation",
                businessException.Message,
                null,
                new Dictionary<string, object?>
                {
                    ["ruleName"] = businessException.RuleName
                }),

            ArgumentException argumentException => (
                StatusCodes.Status400BadRequest,
                "Erro de validação",
                argumentException.Message,
                null,
                new Dictionary<string, object?>
                {
                    ["parameterName"] = argumentException.ParamName
                }),

            DomainException domainException => (
                StatusCodes.Status400BadRequest,
                "Domain Rule Violation",
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
                "Internal Server Error",
                "An unexpected error occurred while processing your request",
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
                "Duplicate Value",
                $"The value for {uniqueException.ColumnName ?? "this field"} already exists",
                null,
                new Dictionary<string, object?>
                {
                    ["constraintName"] = uniqueException.ConstraintName,
                    ["columnName"] = uniqueException.ColumnName
                });
        }

        if (processedException is NotNullConstraintException notNullException)
        {
            return (
                StatusCodes.Status400BadRequest,
                "Required Field Missing",
                $"The field {notNullException.ColumnName ?? "this field"} is required",
                null,
                new Dictionary<string, object?>
                {
                    ["columnName"] = notNullException.ColumnName
                });
        }

        if (processedException is ForeignKeyConstraintException foreignKeyException)
        {
            return (
                StatusCodes.Status400BadRequest,
                "Invalid Reference",
                "The referenced record does not exist",
                null,
                new Dictionary<string, object?>
                {
                    ["constraintName"] = foreignKeyException.ConstraintName,
                    ["tableName"] = foreignKeyException.TableName
                });
        }

        // Fallback para DbUpdateException genérica
        return (
            StatusCodes.Status400BadRequest,
            "Database Error",
            "A database error occurred while processing your request",
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
        500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        _ => "https://tools.ietf.org/html/rfc7231"
    };
}

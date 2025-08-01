namespace MeAjudaAi.Shared.Common;

public record Error(string Message, int StatusCode = 400)
{
    public static Error NotFound(string message) => new(message, 404);
    public static Error BadRequest(string message) => new(message, 400);
    public static Error Unauthorized(string message) => new(message, 401);
    public static Error Forbidden(string message) => new(message, 403);
    public static Error Internal(string message) => new(message, 500);
}
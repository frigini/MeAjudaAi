namespace MeAjudaAi.Web.Admin.Features.Errors;

/// <summary>
/// Set global error action
/// </summary>
public record SetGlobalErrorAction(Exception Exception, string? ComponentName = null, bool IsRecoverable = false);

/// <summary>
/// Clear global error action
/// </summary>
public record ClearGlobalErrorAction();

/// <summary>
/// Retry after error action
/// </summary>
public record RetryAfterErrorAction();

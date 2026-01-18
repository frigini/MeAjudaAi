namespace MeAjudaAi.Web.Admin.Features.Errors;

/// <summary>
/// Ação para definir erro global.
/// </summary>
public record SetGlobalErrorAction(Exception Exception, string? ComponentName = null, bool IsRecoverable = false);

/// <summary>
/// Ação para limpar erro global.
/// </summary>
public record ClearGlobalErrorAction();

/// <summary>
/// Ação para tentar novamente após erro.
/// </summary>
public record RetryAfterErrorAction();

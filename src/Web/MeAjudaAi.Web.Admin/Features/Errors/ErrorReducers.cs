using Fluxor;
using MeAjudaAi.Web.Admin.Services;

namespace MeAjudaAi.Web.Admin.Features.Errors;

/// <summary>
/// Reducers for error state
/// </summary>
public static class ErrorReducers
{
    [ReducerMethod]
    public static ErrorState OnSetGlobalError(ErrorState state, SetGlobalErrorAction action)
    {
        var correlationId = Guid.NewGuid().ToString("N");
        var userMessage = ErrorLoggingService.GetUserFriendlyMessage(action.Exception);

        return state with
        {
            GlobalError = action.Exception,
            CorrelationId = correlationId,
            UserMessage = userMessage,
            TechnicalDetails = $"{action.Exception.GetType().Name}: {action.Exception.Message}\n\n{action.Exception.StackTrace}",
            ShowErrorUI = true,
            OccurredAt = DateTimeOffset.Now,
            ComponentName = action.ComponentName,
            IsRecoverable = action.IsRecoverable || ErrorLoggingService.ShouldRetry(action.Exception)
        };
    }

    [ReducerMethod]
    public static ErrorState OnClearGlobalError(ErrorState state, ClearGlobalErrorAction action)
    {
        return new ErrorState();
    }

    [ReducerMethod]
    public static ErrorState OnRetryAfterError(ErrorState state, RetryAfterErrorAction action)
    {
        // Clear error to allow retry
        return new ErrorState();
    }
}

using Fluxor;

namespace MeAjudaAi.Web.Admin.Features.Errors;

/// <summary>
/// Feature state for global errors
/// </summary>
public class ErrorFeature : Feature<ErrorState>
{
    public override string GetName() => "Error";

    protected override ErrorState GetInitialState()
    {
        return new ErrorState
        {
            GlobalError = null,
            CorrelationId = null,
            UserMessage = null,
            TechnicalDetails = null,
            ShowErrorUI = false,
            OccurredAt = null,
            ComponentName = null,
            IsRecoverable = false
        };
    }
}

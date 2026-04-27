using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Shared.Utilities;

public static class CorrelationHelper
{
    /// <summary>
    /// Obtém o CorrelationId do header ou do TraceIdentifier, gerando um novo se necessário.
    /// </summary>
    public static Guid ParseCorrelationId(HttpContext context)
    {
        var correlationIdHeader = context.Request.Headers[AuthConstants.Headers.CorrelationId].ToString();
        
        if (Guid.TryParse(correlationIdHeader, out var correlationId))
        {
            return correlationId;
        }

        if (!string.IsNullOrEmpty(context.TraceIdentifier) && Guid.TryParse(context.TraceIdentifier, out var traceId))
        {
            return traceId;
        }

        return Guid.NewGuid();
    }
}

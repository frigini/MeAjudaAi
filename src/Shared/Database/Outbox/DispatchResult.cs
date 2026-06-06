using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Database.Outbox;

public abstract partial class OutboxProcessorBase<TMessage> where TMessage : OutboxMessage
{
    /// <summary>
    /// Resultado de um despacho de mensagem.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record DispatchResult(bool IsSuccess, string? ErrorMessage = null, bool IsCanceled = false)
    {
        public static DispatchResult Success() => new(true);
        public static DispatchResult Failure(string errorMessage) => new(false, errorMessage);
        public static DispatchResult Canceled() => new(false, null, true);
    }
}

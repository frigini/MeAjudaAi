using MeAjudaAi.Shared.Messaging.DeadLetter;

namespace MeAjudaAi.Shared.Messaging.Factories;

/// <summary>
/// Factory para criar o serviço de Dead Letter Queue apropriado baseado no ambiente
/// </summary>
public interface IDeadLetterServiceFactory
{
    /// <summary>
    /// Cria o serviço de DLQ apropriado para o ambiente atual
    /// </summary>
    IDeadLetterService CreateDeadLetterService();
}

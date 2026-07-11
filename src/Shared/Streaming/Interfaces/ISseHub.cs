namespace MeAjudaAi.Shared.Streaming.Interfaces;

public interface ISseHub<T>
{
    Task PublishAsync(string topic, T data, CancellationToken ct = default);
    IAsyncEnumerable<T> SubscribeAsync(string topic, CancellationToken ct = default);
}

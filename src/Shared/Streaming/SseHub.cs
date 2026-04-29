using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace MeAjudaAi.Shared.Streaming;

/// <summary>
/// Interface para um Hub SSE (Server-Sent Events) tipado.
/// </summary>
public interface ISseHub<T>
{
    /// <summary>
    /// Publica um evento para todos os assinantes de um tópico.
    /// </summary>
    Task PublishAsync(string topic, T data, CancellationToken ct = default);

    /// <summary>
    /// Subscreve a um fluxo de eventos para um tópico específico.
    /// </summary>
    IAsyncEnumerable<T> SubscribeAsync(string topic, CancellationToken ct = default);
}

/// <summary>
/// Implementação em memória de um Hub SSE usando Channels.
/// </summary>
public class SseHub<T> : ISseHub<T>
{
    private readonly ConcurrentDictionary<string, ConcurrentBag<Channel<T>>> _topics = new();

    public Task PublishAsync(string topic, T data, CancellationToken ct = default)
    {
        if (_topics.TryGetValue(topic, out var channels))
        {
            foreach (var channel in channels)
            {
                channel.Writer.TryWrite(data);
            }
        }

        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<T> SubscribeAsync(string topic, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        var channels = _topics.GetOrAdd(topic, _ => new ConcurrentBag<Channel<T>>());
        channels.Add(channel);

        try
        {
            while (await channel.Reader.WaitToReadAsync(ct))
            {
                while (channel.Reader.TryRead(out var data))
                {
                    yield return data;
                }
            }
        }
        finally
        {
            // Nota: Em uma implementação real, deveríamos remover o channel do ConcurrentBag.
            // Para este piloto, manteremos simples.
        }
    }
}

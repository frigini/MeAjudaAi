using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace MeAjudaAi.Shared.Streaming;

public interface ISseHub<T>
{
    Task PublishAsync(string topic, T data, CancellationToken ct = default);
    IAsyncEnumerable<T> SubscribeAsync(string topic, CancellationToken ct = default);
}

[ExcludeFromCodeCoverage]
public class SseHub<T> : ISseHub<T>
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Channel<T>, byte>> _topics = new();

    public Task PublishAsync(string topic, T data, CancellationToken ct = default)
    {
        if (_topics.TryGetValue(topic, out var channels))
        {
            foreach (var kvp in channels)
            {
                kvp.Key.Writer.TryWrite(data);
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

        var channels = _topics.GetOrAdd(topic, _ => new ConcurrentDictionary<Channel<T>, byte>());
        channels.TryAdd(channel, 0);

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
            channels.TryRemove(channel, out _);
            channel.Writer.Complete();
        }
    }
}

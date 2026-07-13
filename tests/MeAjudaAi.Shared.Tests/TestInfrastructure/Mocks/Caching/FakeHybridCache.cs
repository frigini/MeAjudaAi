using Microsoft.Extensions.Caching.Hybrid;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Caching;

/// <summary>
/// Fake de HybridCache para testes unitários.
/// Rastreia chamadas e permite simular hits, misses e erros.
/// </summary>
public class FakeHybridCache : HybridCache
{
    public bool SimulateCacheHit { get; set; }
    public object? HitValue { get; set; }
    public bool GetOrCreateAsyncCalled { get; set; }
    public bool SetAsyncCalled { get; set; }
    public bool RemoveAsyncCalled { get; set; }
    public bool RemoveByTagAsyncCalled { get; set; }
    public string? LastKey { get; set; }
    public object? LastValue { get; set; }
    public Exception? ExceptionToThrow { get; set; }

    public override ValueTask<T> GetOrCreateAsync<TState, T>(
        string key,
        TState state,
        Func<TState, CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        if (ExceptionToThrow != null)
            throw ExceptionToThrow;

        GetOrCreateAsyncCalled = true;
        LastKey = key;

        if (SimulateCacheHit)
            return new ValueTask<T>((T)HitValue!);

        return factory(state, cancellationToken);
    }

    public override ValueTask SetAsync<T>(
        string key,
        T value,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        if (ExceptionToThrow != null)
            throw ExceptionToThrow;

        SetAsyncCalled = true;
        LastKey = key;
        LastValue = value;
        return ValueTask.CompletedTask;
    }

    public override ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (ExceptionToThrow != null)
            throw ExceptionToThrow;

        RemoveAsyncCalled = true;
        LastKey = key;
        return ValueTask.CompletedTask;
    }

    public override ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        if (ExceptionToThrow != null)
            throw ExceptionToThrow;

        RemoveByTagAsyncCalled = true;
        LastKey = tag;
        return ValueTask.CompletedTask;
    }
}

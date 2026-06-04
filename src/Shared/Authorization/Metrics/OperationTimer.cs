using System.Diagnostics;

namespace MeAjudaAi.Shared.Authorization.Metrics;

/// <summary>
/// Timer para medir duração de operações.
/// </summary>
public sealed class OperationTimer : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly Action<TimeSpan> _onComplete;
    private bool _disposed;

    public OperationTimer(Action onStart, Action<TimeSpan> onComplete)
    {
        ArgumentNullException.ThrowIfNull(onStart);
        ArgumentNullException.ThrowIfNull(onComplete);
        
        _onComplete = onComplete;
        _stopwatch = Stopwatch.StartNew();
        onStart();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _stopwatch.Stop();
        _onComplete(_stopwatch.Elapsed);
    }
}

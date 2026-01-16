using System.Timers;
using Timer = System.Timers.Timer;

namespace MeAjudaAi.Web.Admin.Helpers;

/// <summary>
/// Helper class for debouncing function calls (e.g., search input)
/// </summary>
public class DebounceHelper : IDisposable
{
    private readonly Timer _timer;
    private readonly int _milliseconds;

    public DebounceHelper(int milliseconds = 300)
    {
        _milliseconds = milliseconds;
        _timer = new Timer(_milliseconds);
        _timer.AutoReset = false;
    }

    /// <summary>
    /// Debounce an action - will only execute after specified delay without new calls
    /// </summary>
    public void Debounce(Action action)
    {
        _timer.Stop();
        _timer.Elapsed -= OnTimerElapsed;
        
        void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            action();
        }
        
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}

/// <summary>
/// Extension methods for debouncing
/// </summary>
public static class DebounceExtensions
{
    private static readonly Dictionary<string, DebounceHelper> Debouncers = new();

    /// <summary>
    /// Debounce an action with a specific key
    /// </summary>
    public static void DebounceAction(this string key, Action action, int milliseconds = 300)
    {
        if (!Debouncers.ContainsKey(key))
        {
            Debouncers[key] = new DebounceHelper(milliseconds);
        }

        Debouncers[key].Debounce(action);
    }
}

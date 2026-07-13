namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Helpers;

/// <summary>
/// Utility that saves and restores environment variable values during tests.
/// Tracks which variables were modified and restores them on Dispose.
/// </summary>
public sealed class EnvironmentVariableRestorer : IDisposable
{
    private readonly Dictionary<string, string?> _originalValues = new();

    public void SetVariable(string name, string? value)
    {
        if (!_originalValues.ContainsKey(name))
        {
            _originalValues[name] = Environment.GetEnvironmentVariable(name);
        }

        Environment.SetEnvironmentVariable(name, value);
    }

    public string? GetVariable(string name) =>
        Environment.GetEnvironmentVariable(name);

    public void Dispose()
    {
        foreach (var (name, originalValue) in _originalValues)
        {
            Environment.SetEnvironmentVariable(name, originalValue);
        }
        _originalValues.Clear();
    }
}

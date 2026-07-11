namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Helpers;

/// <summary>
/// Utility that saves and restores environment variable values during tests.
/// Tracks which variables were modified and restores them on Dispose.
/// </summary>
public sealed class EnvironmentVariableRestorer : IDisposable
{
    private readonly HashSet<string> _modifiedVariables = new();

    public void SetVariable(string name, string? value)
    {
        _modifiedVariables.Add(name);
        Environment.SetEnvironmentVariable(name, value);
    }

    public string? GetVariable(string name) =>
        Environment.GetEnvironmentVariable(name);

    public void Dispose()
    {
        foreach (var name in _modifiedVariables)
        {
            Environment.SetEnvironmentVariable(name, null);
        }
        _modifiedVariables.Clear();
    }
}

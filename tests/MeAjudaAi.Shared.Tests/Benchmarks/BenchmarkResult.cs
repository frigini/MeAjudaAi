namespace MeAjudaAi.Shared.Tests.Performance;

/// <summary>
/// Resultado de um benchmark
/// </summary>
public class BenchmarkResult
{
    public string OperationName { get; set; } = string.Empty;
    public long ElapsedMilliseconds { get; set; }
    public long MemoryUsedBytes { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
}

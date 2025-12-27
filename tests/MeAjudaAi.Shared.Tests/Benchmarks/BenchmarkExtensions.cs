namespace MeAjudaAi.Shared.Tests.Performance;

/// <summary>
/// Extensões para facilitar uso de benchmarking em testes
/// </summary>
public static class BenchmarkExtensions
{
    /// <summary>
    /// Benchmark rápido para uma operação em teste
    /// </summary>
    public static async Task<T> BenchmarkOperationAsync<T>(
        this ITestOutputHelper output,
        string operationName,
        Func<Task<T>> operation,
        long? expectedMaxMs = null)
    {
        var benchmark = new TestPerformanceBenchmark(output);
        var result = await benchmark.BenchmarkAsync(operationName, operation);

        if (expectedMaxMs.HasValue)
        {
            var actualMs = benchmark.GetResult(operationName)?.ElapsedMilliseconds ?? 0;
            if (actualMs > expectedMaxMs.Value)
            {
                output.WriteLine($"⚠️ PERFORMANCE WARNING: {operationName} took {actualMs}ms, expected <{expectedMaxMs}ms");
            }
        }

        return result;
    }
}

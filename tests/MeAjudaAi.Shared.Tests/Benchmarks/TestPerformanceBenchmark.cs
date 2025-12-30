using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.Performance;

/// <summary>
/// Utilitário para benchmarking de performance dos testes
/// </summary>
public class TestPerformanceBenchmark(ITestOutputHelper output, ILogger? logger = null)
{
    private readonly Dictionary<string, BenchmarkResult> _results = new();

    /// <summary>
    /// Executa benchmark de uma operação
    /// </summary>
    public async Task<T> BenchmarkAsync<T>(string operationName, Func<Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        var memoryBefore = GC.GetTotalMemory(false);

        try
        {
            var result = await operation();
            stopwatch.Stop();

            var memoryAfter = GC.GetTotalMemory(false);
            var memoryUsed = memoryAfter - memoryBefore;

            var benchmarkResult = new BenchmarkResult
            {
                OperationName = operationName,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                MemoryUsedBytes = memoryUsed,
                Success = true,
                Timestamp = DateTime.UtcNow
            };

            _results[operationName] = benchmarkResult;
            LogResult(benchmarkResult);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var benchmarkResult = new BenchmarkResult
            {
                OperationName = operationName,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                MemoryUsedBytes = 0,
                Success = false,
                ErrorMessage = ex.Message,
                Timestamp = DateTime.UtcNow
            };

            _results[operationName] = benchmarkResult;
            LogResult(benchmarkResult);

            throw;
        }
    }

    /// <summary>
    /// Gera relatório de performance
    /// </summary>
    public void GenerateReport()
    {
        if (!_results.Any())
        {
            output.WriteLine("Nenhum benchmark foi executado.");
            return;
        }

        output.WriteLine("\n=== RELATÓRIO DE PERFORMANCE ===");
        output.WriteLine($"Total de operações: {_results.Count}");
        output.WriteLine($"Tempo total: {_results.Sum(r => r.Value.ElapsedMilliseconds)}ms");
        output.WriteLine("");

        foreach (var result in _results.Values.OrderByDescending(r => r.ElapsedMilliseconds))
        {
            var status = result.Success ? "✅" : "❌";
            output.WriteLine($"{status} {result.OperationName}: {result.ElapsedMilliseconds}ms");
        }
    }

    /// <summary>
    /// Compara performance com baseline esperado
    /// </summary>
    public void CompareWithBaseline(Dictionary<string, long> baselineMs)
    {
        output.WriteLine("\n=== COMPARAÇÃO COM BASELINE ===");

        foreach (var baseline in baselineMs)
        {
            if (_results.TryGetValue(baseline.Key, out var result))
            {
                var improvement = ((double)(baseline.Value - result.ElapsedMilliseconds) / baseline.Value) * 100;
                var icon = improvement > 0 ? "🚀" : "🐌";
                var sign = improvement > 0 ? "+" : "";

                output.WriteLine($"{icon} {baseline.Key}: {sign}{improvement:F1}%");
            }
        }
    }

    private void LogResult(BenchmarkResult result)
    {
        output.WriteLine($"⏱️ {result.OperationName}: {result.ElapsedMilliseconds}ms");
        logger?.LogInformation($"Benchmark '{result.OperationName}': {result.ElapsedMilliseconds}ms");
    }

    public BenchmarkResult? GetResult(string operationName)
    {
        _results.TryGetValue(operationName, out var result);
        return result;
    }
}

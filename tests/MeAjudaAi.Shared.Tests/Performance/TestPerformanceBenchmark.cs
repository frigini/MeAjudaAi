using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace MeAjudaAi.Shared.Tests.Performance;

/// <summary>
/// Utilitário para benchmarking de performance dos testes
/// </summary>
public class TestPerformanceBenchmark
{
    private readonly ITestOutputHelper _output;
    private readonly ILogger? _logger;
    private readonly Dictionary<string, BenchmarkResult> _results = new();
    
    public TestPerformanceBenchmark(ITestOutputHelper output, ILogger? logger = null)
    {
        _output = output;
        _logger = logger;
    }

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
            _output.WriteLine("Nenhum benchmark foi executado.");
            return;
        }

        _output.WriteLine("\n=== RELATÓRIO DE PERFORMANCE ===");
        _output.WriteLine($"Total de operações: {_results.Count}");
        _output.WriteLine($"Tempo total: {_results.Sum(r => r.Value.ElapsedMilliseconds)}ms");
        _output.WriteLine("");

        foreach (var result in _results.Values.OrderByDescending(r => r.ElapsedMilliseconds))
        {
            var status = result.Success ? "✅" : "❌";
            _output.WriteLine($"{status} {result.OperationName}: {result.ElapsedMilliseconds}ms");
        }
    }

    /// <summary>
    /// Compara performance com baseline esperado
    /// </summary>
    public void CompareWithBaseline(Dictionary<string, long> baselineMs)
    {
        _output.WriteLine("\n=== COMPARAÇÃO COM BASELINE ===");
        
        foreach (var baseline in baselineMs)
        {
            if (_results.TryGetValue(baseline.Key, out var result))
            {
                var improvement = ((double)(baseline.Value - result.ElapsedMilliseconds) / baseline.Value) * 100;
                var icon = improvement > 0 ? "🚀" : "🐌";
                var sign = improvement > 0 ? "+" : "";
                
                _output.WriteLine($"{icon} {baseline.Key}: {sign}{improvement:F1}%");
            }
        }
    }

    private void LogResult(BenchmarkResult result)
    {
        _output.WriteLine($"⏱️ {result.OperationName}: {result.ElapsedMilliseconds}ms");
        _logger?.LogInformation($"Benchmark '{result.OperationName}': {result.ElapsedMilliseconds}ms");
    }

    public BenchmarkResult? GetResult(string operationName)
    {
        _results.TryGetValue(operationName, out var result);
        return result;
    }
}

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
using MeAjudaAi.Shared.Tests.Fixtures;
using MeAjudaAi.Shared.Tests.Performance;
using Xunit.Abstractions;

namespace MeAjudaAi.Shared.Tests.Examples;

/// <summary>
/// Exemplo demonstrativo de como implementar testes de performance
/// usando fixtures compartilhados e benchmarking.
/// Este arquivo serve como documentação/exemplo - pode ser removido em produção.
/// </summary>
[Collection("Parallel")]
public class PerformanceTestingExample(ITestOutputHelper output)
{
    private readonly TestPerformanceBenchmark _benchmark = new(output);

    [Fact]
    public async Task FastUnitTest_ShouldCompleteQuickly()
    {
        // Este teste usa o fixture compartilhado e mede performance
        var result = await _benchmark.BenchmarkAsync("SimpleOperation", async () =>
        {
            // Simula operação rápida
            await Task.Delay(10);
            return "success";
        });

        result.Should().Be("success");
        _benchmark.GenerateReport();
        
        // Verifica se está dentro do esperado (< 50ms)
        var benchmarkResult = _benchmark.GetResult("SimpleOperation");
        benchmarkResult.Should().NotBeNull();
        benchmarkResult!.ElapsedMilliseconds.Should().BeLessThan(50);
    }

    [Fact]
    public async Task ParallelizableTest_ShouldRunInParallel()
    {
        // Este teste pode rodar em paralelo com outros da mesma collection
        var result = await output.BenchmarkOperationAsync(
            "ParallelOperation",
            async () =>
            {
                await Task.Delay(20);
                return 42;
            },
            expectedMaxMs: 100
        );

        result.Should().Be(42);
    }

    [Fact]
    public async Task PerformanceBaseline_ShouldMeetExpectations()
    {
        // Testa múltiplas operações e compara com baseline
        await _benchmark.BenchmarkAsync("Operation1", async () =>
        {
            await Task.Delay(5);
            return true;
        });

        await _benchmark.BenchmarkAsync("Operation2", async () =>
        {
            await Task.Delay(15);
            return true;
        });

        // Compara com baseline esperado
        _benchmark.CompareWithBaseline(new Dictionary<string, long>
        {
            { "Operation1", 20 }, // Esperamos que seja mais rápido que 20ms
            { "Operation2", 30 }  // Esperamos que seja mais rápido que 30ms
        });

        _benchmark.GenerateReport();
    }
}
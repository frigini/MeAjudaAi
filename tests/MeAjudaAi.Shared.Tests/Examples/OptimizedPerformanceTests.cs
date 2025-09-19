using MeAjudaAi.Shared.Tests.Collections;
using MeAjudaAi.Shared.Tests.Fixtures;
using MeAjudaAi.Shared.Tests.Performance;
using Xunit.Abstractions;

namespace MeAjudaAi.Shared.Tests.Examples;

/// <summary>
/// Exemplo de teste otimizado usando fixtures compartilhados e benchmarking
/// </summary>
[Collection("Parallel")]
public class OptimizedPerformanceTests : IClassFixture<SharedTestFixture>
{
    private readonly SharedTestFixture _fixture;
    private readonly ITestOutputHelper _output;
    private readonly TestPerformanceBenchmark _benchmark;

    public OptimizedPerformanceTests(SharedTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _benchmark = new TestPerformanceBenchmark(output);
    }

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
        var result = await _output.BenchmarkOperationAsync(
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
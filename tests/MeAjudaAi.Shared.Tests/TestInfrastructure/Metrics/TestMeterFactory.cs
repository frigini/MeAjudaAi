using System.Diagnostics.Metrics;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Metrics;

/// <summary>
/// IMeterFactory de teste para criar métricas em testes
/// </summary>
internal sealed class TestMeterFactory : IMeterFactory
{
    private readonly List<Meter> _meters = new();

    public Meter Create(MeterOptions options)
    {
        var meter = new Meter(options);
        _meters.Add(meter);
        return meter;
    }

    public void Dispose()
    {
        foreach (var meter in _meters)
        {
            meter.Dispose();
        }
        _meters.Clear();
    }
}

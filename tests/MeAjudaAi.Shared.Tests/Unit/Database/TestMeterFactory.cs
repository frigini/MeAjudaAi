using System.Diagnostics.Metrics;

namespace MeAjudaAi.Shared.Tests.Unit.Database;

/// <summary>
/// IMeterFactory de teste para criar métricas em testes
/// </summary>
internal class TestMeterFactory : IMeterFactory
{
    public Meter Create(MeterOptions options)
    {
        return new Meter(options.Name);
    }

    public void Dispose()
    {
        // Nada para dispose
    }
}

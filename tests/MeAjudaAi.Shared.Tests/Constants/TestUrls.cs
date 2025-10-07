namespace MeAjudaAi.Tests.Shared.Constants;

/// <summary>
/// Constantes para URLs e configurações de teste
/// </summary>
public static class TestUrls
{
    public const string LocalhostKeycloak = "http://localhost:8080";
    public const string LocalhostTelemetry = "http://localhost:4317";
    public const string LocalhostDatabase = "Host=localhost;Port=5432;Database=meajudaai_mock;Username=postgres;Password=test;";
    public const string LocalhostRabbitMq = "amqp://localhost";
}
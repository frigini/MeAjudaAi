namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Options;

/// <summary>
/// Configurações de serviços externos (Keycloak, etc.)
/// </summary>
public class TestExternalServicesOptions
{
    /// <summary>
    /// Se deve usar mocks para Keycloak
    /// </summary>
    public bool UseKeycloakMock { get; set; } = true;

    /// <summary>
    /// Se deve usar mocks para message bus
    /// </summary>
    public bool UseMessageBusMock { get; set; } = true;
}

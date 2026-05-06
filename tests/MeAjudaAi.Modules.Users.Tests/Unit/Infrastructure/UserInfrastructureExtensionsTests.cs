using System.Collections.Generic;
using FluentAssertions;
using MeAjudaAi.Modules.Users.Infrastructure;
using MeAjudaAi.Modules.Users.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure;

public class UserInfrastructureExtensionsTests
{
    [Theory]
    [InlineData(false, null, null, null, null, true)]   // disabled → mock
    [InlineData(true, null, null, null, null, true)]    // enabled sem config → mock
    [InlineData(true, "http://k", "r", "id", "sec", false)] // enabled + config → real
    public void AddInfrastructure_ShouldWireCorrectKeycloak(bool enabled, string? baseUrl, string? realm, string? clientId, string? secret, bool expectMock)
    {
        var services = new ServiceCollection();
        var configData = new Dictionary<string, string?>
        {
            ["Keycloak:Enabled"] = enabled.ToString(),
            ["Keycloak:BaseUrl"] = baseUrl,
            ["Keycloak:Realm"] = realm,
            ["Keycloak:ClientId"] = clientId,
            ["Keycloak:ClientSecret"] = secret
        };
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();
        
        services.AddInfrastructure(cfg);
        var sp = services.BuildServiceProvider();

        var svc = sp.GetRequiredService<IKeycloakService>();
        if (expectMock)
        {
            svc.Should().BeOfType<MockKeycloakService>();
        }
        else
        {
            svc.Should().BeOfType<KeycloakService>();
        }
    }
}

using FluentAssertions;
using MeAjudaAi.Gateway.Options;

namespace MeAjudaAi.Gateway.Tests.Unit.Options;

[Trait("Category", "Unit")]
[Trait("Layer", "Gateway")]
public class EdgeAuthGuardOptionsTests
{
    [Fact]
    public void EdgeAuthGuardOptions_DefaultValues_ShouldBeInitialized()
    {
        var options = new EdgeAuthGuardOptions();

        options.Should().NotBeNull();
        options.Enabled.Should().BeTrue();
        options.PublicRoutes.Should().NotBeEmpty();
        options.PublicRoutes.Should().Contain("/health");
        options.PublicRoutes.Should().Contain("/swagger");
        options.ChallengeHeader.Should().Be("X-Gateway-Challenge");
        options.AuthenticatedHeader.Should().Be("X-Gateway-Authenticated");
    }

    [Fact]
    public void EdgeAuthGuardOptions_SectionName_ShouldBeEdgeAuthGuard()
    {
        EdgeAuthGuardOptions.SectionName.Should().Be("EdgeAuthGuard");
    }

    [Fact]
    public void EdgeAuthGuardOptions_WithCustomPublicRoutes_ShouldConfigureCorrectly()
    {
        var customRoutes = new List<string>
        {
            "/api/v1/public",
            "/api/v1/auth/login",
            "/health"
        };

        var options = new EdgeAuthGuardOptions
        {
            Enabled = false,
            PublicRoutes = customRoutes,
            ChallengeHeader = "X-Custom-Challenge",
            AuthenticatedHeader = "X-Custom-Auth"
        };

        options.Enabled.Should().BeFalse();
        options.PublicRoutes.Should().HaveCount(3);
        options.ChallengeHeader.Should().Be("X-Custom-Challenge");
        options.AuthenticatedHeader.Should().Be("X-Custom-Auth");
    }
}
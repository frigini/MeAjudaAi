using FluentAssertions;
using MeAjudaAi.ApiService.Options;

namespace MeAjudaAi.ApiService.Tests.Unit.Options;

[Trait("Category", "Unit")]
[Trait("Layer", "ApiService")]
public class SecurityOptionsTests
{
    [Fact]
    public void SecurityOptions_DefaultValues_ShouldBeInitialized()
    {
        var options = new SecurityOptions();

        options.Should().NotBeNull();
        options.EnforceHttps.Should().BeFalse();
        options.EnableStrictTransportSecurity.Should().BeFalse();
        options.AllowedHosts.Should().NotBeNull();
        options.AllowedHosts.Should().BeEmpty();
    }

    [Fact]
    public void SecurityOptions_EnforceHttps_CanBeSetAndRetrieved()
    {
        var options = new SecurityOptions();

        options.EnforceHttps = true;

        options.EnforceHttps.Should().BeTrue();
    }

    [Fact]
    public void SecurityOptions_EnableStrictTransportSecurity_CanBeSetAndRetrieved()
    {
        var options = new SecurityOptions();

        options.EnableStrictTransportSecurity = true;

        options.EnableStrictTransportSecurity.Should().BeTrue();
    }

    [Fact]
    public void SecurityOptions_AllowedHosts_CanBeSetAndRetrieved()
    {
        var options = new SecurityOptions();
        var expectedHosts = new List<string> { "localhost", "example.com", "*.meajudaai.com" };

        options.AllowedHosts = expectedHosts;

        options.AllowedHosts.Should().NotBeNull();
        options.AllowedHosts.Should().HaveCount(3);
        options.AllowedHosts.Should().ContainInOrder(expectedHosts);
    }
}
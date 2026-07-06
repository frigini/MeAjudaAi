using MeAjudaAi.ApiService.Options;

namespace MeAjudaAi.ApiService.Tests.Unit.Options;

[Trait("Category", "Unit")]
[Trait("Layer", "ApiService")]
public class SecurityOptionsTests
{
    [Fact]
    public void SecurityOptions_DefaultValues_ShouldBeInitialized()
    {
        // Arrange & Act
        var options = new SecurityOptions();

        // Assert
        options.Should().NotBeNull();
        options.EnforceHttps.Should().BeFalse();
        options.EnableStrictTransportSecurity.Should().BeFalse();
        options.AllowedHosts.Should().NotBeNull();
        options.AllowedHosts.Should().BeEmpty();
    }

    [Fact]
    public void SecurityOptions_EnforceHttps_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new SecurityOptions();

        // Act
        options.EnforceHttps = true;

        // Assert
        options.EnforceHttps.Should().BeTrue();
    }

    [Fact]
    public void SecurityOptions_EnableStrictTransportSecurity_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new SecurityOptions();

        // Act
        options.EnableStrictTransportSecurity = true;

        // Assert
        options.EnableStrictTransportSecurity.Should().BeTrue();
    }

    [Fact]
    public void SecurityOptions_AllowedHosts_CanBeSetAndRetrieved()
    {
        // Arrange
        var options = new SecurityOptions();
        var expectedHosts = new List<string> { "localhost", "example.com", "*.meajudaai.com" };

        // Act
        options.AllowedHosts = expectedHosts;

        // Assert
        options.AllowedHosts.Should().NotBeNull();
        options.AllowedHosts.Should().HaveCount(3);
        options.AllowedHosts.Should().ContainInOrder(expectedHosts);
    }
}

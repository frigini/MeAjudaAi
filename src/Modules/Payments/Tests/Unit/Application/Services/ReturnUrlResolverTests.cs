using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Payments.Application.Options;
using MeAjudaAi.Modules.Payments.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Application.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Payments")]
[Trait("Layer", "Application")]
public class ReturnUrlResolverTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly PaymentsOptions _options;
    private readonly Mock<IHostEnvironment> _environmentMock;
    private readonly Mock<ILogger<ReturnUrlResolver>> _loggerMock;
    private readonly ReturnUrlResolver _resolver;

    public ReturnUrlResolverTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _options = new PaymentsOptions();
        _environmentMock = new Mock<IHostEnvironment>();
        _loggerMock = new Mock<ILogger<ReturnUrlResolver>>();

        _configurationMock.Setup(x => x["ClientBaseUrl"]).Returns("https://meajudaai.com");
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");

        _resolver = new ReturnUrlResolver(
            _configurationMock.Object,
            _options,
            _environmentMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void Resolve_AliasAccount_ShouldReturnClientBaseUrlAccount()
    {
        var result = _resolver.Resolve("account", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("https://meajudaai.com/account");
    }

    [Fact]
    public void Resolve_AliasBilling_ShouldReturnClientBaseUrlBilling()
    {
        var result = _resolver.Resolve("billing", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("https://meajudaai.com/billing");
    }

    [Theory]
    [InlineData("ACCOUNT", "/account")]
    [InlineData("Billing", "/billing")]
    [InlineData("  account  ", "/account")]
    public void Resolve_AliasCaseInsensitive_ShouldResolve(string alias, string expectedPath)
    {
        var result = _resolver.Resolve(alias, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be($"https://meajudaai.com{expectedPath}");
    }

    [Fact]
    public void Resolve_Null_ShouldFallbackToClientBaseUrl()
    {
        var result = _resolver.Resolve(null, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("https://meajudaai.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Resolve_EmptyOrWhitespace_ShouldFallbackToClientBaseUrl(string url)
    {
        var result = _resolver.Resolve(url, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("https://meajudaai.com");
    }

    [Fact]
    public void Resolve_InvalidUrl_ShouldFallbackToClientBaseUrl()
    {
        var result = _resolver.Resolve("not-a-url", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("https://meajudaai.com");
    }

    [Fact]
    public void Resolve_TrustedHostHttps_ShouldReturnSameUrl()
    {
        _options.AllowedReturnHosts = new[] { "trusted.com" };
        var url = "https://trusted.com/billing";

        var result = _resolver.Resolve(url, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(url);
    }

    [Fact]
    public void Resolve_ClientBaseUrlHost_ShouldBeTrusted()
    {
        var url = "https://meajudaai.com/account";

        var result = _resolver.Resolve(url, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(url);
    }

    [Fact]
    public void Resolve_UntrustedHost_ShouldFallbackToClientBaseUrl()
    {
        _options.AllowedReturnHosts = Array.Empty<string>();
        var url = "https://evil.com/steal";

        var result = _resolver.Resolve(url, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("https://meajudaai.com");
    }

    [Fact]
    public void Resolve_HttpNonLocalhost_ShouldFallbackToClientBaseUrl()
    {
        var url = "http://meajudaai.com/account";

        var result = _resolver.Resolve(url, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("https://meajudaai.com");
    }

    [Fact]
    public void Resolve_AllowedReturnHostsPlusClientBaseUrl_ShouldTrustBoth()
    {
        _options.AllowedReturnHosts = new[] { "external.com" };

        var result1 = _resolver.Resolve("https://external.com/path", Guid.NewGuid());
        var result2 = _resolver.Resolve("https://meajudaai.com/path", Guid.NewGuid());

        result1.Value.Should().Be("https://external.com/path");
        result2.Value.Should().Be("https://meajudaai.com/path");
    }

    [Fact]
    public void Resolve_ClientBaseUrlNotConfigured_ShouldReturnFailure()
    {
        _configurationMock.Setup(x => x["ClientBaseUrl"]).Returns((string?)null);

        var resolver = new ReturnUrlResolver(
            _configurationMock.Object,
            _options,
            _environmentMock.Object,
            _loggerMock.Object);

        var result = resolver.Resolve("account", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.StatusCode.Should().Be(500);
        result.Error.Message.Should().Contain("ClientBaseUrl");
    }

    [Fact]
    public void Resolve_ClientBaseUrlWithTrailingSlash_ShouldNormalize()
    {
        _configurationMock.Setup(x => x["ClientBaseUrl"]).Returns("https://meajudaai.com/");

        var resolver = new ReturnUrlResolver(
            _configurationMock.Object,
            _options,
            _environmentMock.Object,
            _loggerMock.Object);

        var result = resolver.Resolve("account", Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("https://meajudaai.com/account");
    }
}

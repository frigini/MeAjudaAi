using System.IO.Compression;
using FluentAssertions;
using MeAjudaAi.ApiService.Extensions;
using MeAjudaAi.ApiService.Providers.Compression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MeAjudaAi.ApiService.Tests.Unit.Extensions;

/// <summary>
/// Testes abrangentes para PerformanceExtensions.cs (compressão de resposta, caching)
/// </summary>
public class PerformanceExtensionsTests
{
    #region AddResponseCompression Tests (8 tests)

    [Fact]
    public void AddResponseCompression_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = PerformanceExtensions.AddResponseCompression(services);

        // Assert
        result.Should().BeSameAs(services);
        services.Should().Contain(s => s.ServiceType == typeof(IConfigureOptions<ResponseCompressionOptions>));
    }

    [Fact]
    public void AddResponseCompression_ShouldEnableHttpsCompression()
    {
        // Arrange
        var services = new ServiceCollection();
        PerformanceExtensions.AddResponseCompression(services);

        // Act
        var options = services.BuildServiceProvider()
            .GetRequiredService<IOptions<ResponseCompressionOptions>>().Value;

        // Assert
        options.EnableForHttps.Should().BeTrue();
    }

    [Fact]
    public void AddResponseCompression_ShouldRegisterSafeCompressionProviders()
    {
        // Arrange
        var services = new ServiceCollection();
        PerformanceExtensions.AddResponseCompression(services);

        // Act - Build provider to trigger options configuration
        var options = services.BuildServiceProvider()
            .GetRequiredService<IOptions<ResponseCompressionOptions>>().Value;

        // Assert - Verify both gzip and brotli compression providers are registered
        options.Providers.Should().HaveCount(2, "both gzip and brotli providers should be configured");
    }

    [Fact]
    public void AddResponseCompression_ShouldConfigureGzipCompressionLevel()
    {
        // Arrange
        var services = new ServiceCollection();
        PerformanceExtensions.AddResponseCompression(services);

        // Act - Build provider to trigger options configuration
        var options = services.BuildServiceProvider()
            .GetRequiredService<IOptions<GzipCompressionProviderOptions>>().Value;

        // Assert - Verify Gzip compression level is set to Optimal
        options.Level.Should().Be(CompressionLevel.Optimal);
    }

    [Fact]
    public void AddResponseCompression_ShouldConfigureJsonMimeType()
    {
        // Arrange
        var services = new ServiceCollection();
        PerformanceExtensions.AddResponseCompression(services);

        // Act
        var options = services.BuildServiceProvider()
            .GetRequiredService<IOptions<ResponseCompressionOptions>>().Value;

        // Assert
        options.MimeTypes.Should().Contain("application/json");
    }

    [Fact]
    public void AddResponseCompression_ShouldConfigureAllMimeTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        PerformanceExtensions.AddResponseCompression(services);

        // Act
        var options = services.BuildServiceProvider()
            .GetRequiredService<IOptions<ResponseCompressionOptions>>().Value;

        // Assert
        options.MimeTypes.Should().Contain(new[]
        {
            "application/json",
            "application/xml",
            "text/xml",
            "application/javascript",
            "text/css",
            "text/plain"
        });
    }

    [Fact]
    public void AddResponseCompression_ShouldConfigureBrotliOptimalLevel()
    {
        // Arrange
        var services = new ServiceCollection();
        PerformanceExtensions.AddResponseCompression(services);

        // Act
        var provider = services.BuildServiceProvider();
        var brotliOptions = provider.GetRequiredService<IOptions<BrotliCompressionProviderOptions>>().Value;

        // Assert
        brotliOptions.Level.Should().Be(CompressionLevel.Optimal);
    }

    #endregion

    #region SafeGzipCompressionProvider Tests (4 tests)

    [Fact]
    public void SafeGzipProvider_EncodingName_ShouldBeGzip()
    {
        // Arrange
        var provider = new SafeGzipCompressionProvider();

        // Act & Assert
        provider.EncodingName.Should().Be("gzip");
    }

    [Fact]
    public void SafeGzipProvider_SupportsFlush_ShouldBeTrue()
    {
        // Arrange
        var provider = new SafeGzipCompressionProvider();

        // Act & Assert
        provider.SupportsFlush.Should().BeTrue();
    }

    [Fact]
    public void SafeGzipProvider_CreateStream_ShouldReturnGZipStream()
    {
        // Arrange
        var provider = new SafeGzipCompressionProvider();
        using var outputStream = new MemoryStream();

        // Act
        using var compressionStream = provider.CreateStream(outputStream);

        // Assert
        compressionStream.Should().BeOfType<GZipStream>();
    }

    #endregion

    #region SafeBrotliCompressionProvider Tests (3 tests)

    [Fact]
    public void SafeBrotliProvider_EncodingName_ShouldBeBr()
    {
        // Arrange
        var provider = new SafeBrotliCompressionProvider();

        // Act & Assert
        provider.EncodingName.Should().Be("br");
    }

    [Fact]
    public void SafeBrotliProvider_SupportsFlush_ShouldBeTrue()
    {
        // Arrange
        var provider = new SafeBrotliCompressionProvider();

        // Act & Assert
        provider.SupportsFlush.Should().BeTrue();
    }

    [Fact]
    public void SafeBrotliProvider_CreateStream_ShouldReturnBrotliStream()
    {
        // Arrange
        var provider = new SafeBrotliCompressionProvider();
        using var outputStream = new MemoryStream();

        // Act
        using var compressionStream = provider.CreateStream(outputStream);

        // Assert
        compressionStream.Should().BeOfType<BrotliStream>();
    }

    #endregion

    #region AddStaticFilesWithCaching Tests (2 tests)

    [Fact]
    public void AddStaticFilesWithCaching_ShouldBeChainable()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddStaticFilesWithCaching();

        // Assert
        result.Should().BeSameAs(services);
        // Note: Static files configuration is internal to ASP.NET Core, so only chainability can be verified
    }

    #endregion

    #region AddApiResponseCaching Tests (4 tests)

    [Fact]
    public void AddApiResponseCaching_ShouldBeChainable()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddApiResponseCaching();

        // Assert
        result.Should().BeSameAs(services);
        // Note: ResponseCachingOptions is internal to ASP.NET Core, so only chainability can be verified
    }

    #endregion

    #region Helper Methods

    private static HttpContext CreateHttpContext(string path = "/api/test")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Headers.Clear();
        context.Response.Headers.Clear();
        return context;
    }

    private class TestRequestCookieCollection : IRequestCookieCollection
    {
        private readonly Dictionary<string, string> _cookies;

        public TestRequestCookieCollection(Dictionary<string, string> cookies)
        {
            _cookies = cookies;
        }

        public string? this[string key] => _cookies.TryGetValue(key, out var value) ? value : null;
        public int Count => _cookies.Count;
        public ICollection<string> Keys => _cookies.Keys;
        public bool ContainsKey(string key) => _cookies.ContainsKey(key);
        public bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value) => _cookies.TryGetValue(key, out value!);
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _cookies.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _cookies.GetEnumerator();
    }

    #endregion
}


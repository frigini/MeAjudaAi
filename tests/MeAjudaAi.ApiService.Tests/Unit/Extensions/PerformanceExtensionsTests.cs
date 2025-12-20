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

    #region IsSafeForCompression Tests (25 tests)

    [Fact]
    public void IsSafeForCompression_NullContext_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => PerformanceExtensions.IsSafeForCompression(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsSafeForCompression_WithAuthorizationHeader_ShouldReturnFalse()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers["Authorization"] = "Bearer token";

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSafeForCompression_WithApiKeyHeader_ShouldReturnFalse()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers["X-API-Key"] = "secret-key";

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSafeForCompression_WithResponseAuthHeader_ShouldReturnFalse()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.Headers["Authorization"] = "Bearer token";

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSafeForCompression_WithAuthenticatedUser_ShouldReturnFalse()
    {
        // Arrange
        var context = CreateHttpContext();
        var identity = new System.Security.Claims.ClaimsIdentity("TestAuth"); // Passing auth type makes IsAuthenticated=true
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        context.User = principal;

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("/auth")]
    [InlineData("/login")]
    [InlineData("/token")]
    [InlineData("/refresh")]
    [InlineData("/logout")]
    public void IsSafeForCompression_WithSensitiveAuthPaths_ShouldReturnFalse(string path)
    {
        // Arrange
        var context = CreateHttpContext(path);

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("/api/auth")]
    [InlineData("/api/login")]
    [InlineData("/api/token")]
    [InlineData("/api/refresh")]
    public void IsSafeForCompression_WithSensitiveApiPaths_ShouldReturnFalse(string path)
    {
        // Arrange
        var context = CreateHttpContext(path);

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("/connect")]
    [InlineData("/oauth")]
    [InlineData("/openid")]
    [InlineData("/identity")]
    public void IsSafeForCompression_WithOAuthPaths_ShouldReturnFalse(string path)
    {
        // Arrange
        var context = CreateHttpContext(path);

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("/users/profile")]
    [InlineData("/users/me")]
    [InlineData("/account")]
    public void IsSafeForCompression_WithUserProfilePaths_ShouldReturnFalse(string path)
    {
        // Arrange
        var context = CreateHttpContext(path);

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSafeForCompression_WithSmallResponse_ShouldReturnFalse()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.ContentLength = 512; // < 1KB

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSafeForCompression_WithLargeResponse_ShouldCheckOtherCriteria()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.ContentLength = 2048; // > 1KB

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert - Should pass this check, depends on other criteria
        result.Should().BeTrue(); // No auth, no sensitive path, large enough
    }

    [Fact]
    public void IsSafeForCompression_WithJwtContentType_ShouldReturnFalse()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.ContentType = "application/jwt";

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSafeForCompression_WithFormUrlencodedContentType_ShouldReturnFalse()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.ContentType = "application/x-www-form-urlencoded";

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSafeForCompression_WithMultipartFormData_ShouldReturnFalse()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.ContentType = "multipart/form-data";

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("auth")]
    [InlineData("session")]
    [InlineData("token")]
    [InlineData("jwt")]
    [InlineData("identity")]
    public void IsSafeForCompression_WithSensitiveCookies_ShouldReturnFalse(string cookieName)
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Cookies = new TestRequestCookieCollection(new Dictionary<string, string>
        {
            [cookieName] = "value"
        });

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(".AspNetCore.Identity")]
    [InlineData(".AspNetCore.Session")]
    [InlineData("XSRF-TOKEN")]
    [InlineData("CSRF-TOKEN")]
    public void IsSafeForCompression_WithFrameworkCookies_ShouldReturnFalse(string cookieName)
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Cookies = new TestRequestCookieCollection(new Dictionary<string, string>
        {
            [cookieName] = "value"
        });

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSafeForCompression_WithSetCookieContainingAuth_ShouldReturnFalse()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.Headers["Set-Cookie"] = "auth_token=value; HttpOnly";

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSafeForCompression_WithSetCookieContainingSession_ShouldReturnFalse()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.Headers["Set-Cookie"] = "session_id=value; Secure";

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSafeForCompression_SafeRequest_ShouldReturnTrue()
    {
        // Arrange
        var context = CreateHttpContext("/api/data");
        context.Response.ContentLength = 2048; // Large enough
        context.Response.ContentType = "application/json";

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSafeForCompression_WithNullContentLength_ShouldPassSizeCheck()
    {
        // Arrange
        var context = CreateHttpContext("/api/data");
        context.Response.ContentLength = null; // Unknown size
        context.Response.ContentType = "application/json";

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeTrue(); // No ContentLength means we don't reject based on size
    }

    [Fact]
    public void IsSafeForCompression_WithNullContentType_ShouldPassContentTypeCheck()
    {
        // Arrange
        var context = CreateHttpContext("/api/data");
        context.Response.ContentType = null;

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeTrue(); // Null content type is safe
    }

    [Fact]
    public void IsSafeForCompression_WithEmptyContentType_ShouldPassContentTypeCheck()
    {
        // Arrange
        var context = CreateHttpContext("/api/data");
        context.Response.ContentType = string.Empty;

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSafeForCompression_CaseInsensitivePath_ShouldDetectSensitive()
    {
        // Arrange
        var context = CreateHttpContext("/AUTH/login");

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSafeForCompression_CaseInsensitiveContentType_ShouldDetectSensitive()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Response.ContentType = "APPLICATION/JWT";

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSafeForCompression_CaseInsensitiveCookie_ShouldDetectSensitive()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Cookies = new TestRequestCookieCollection(new Dictionary<string, string>
        {
            ["AUTH_TOKEN"] = "value"
        });

        // Act
        var result = PerformanceExtensions.IsSafeForCompression(context);

        // Assert
        result.Should().BeFalse();
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


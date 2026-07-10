using MeAjudaAi.ApiService.Providers.Compression;
using System.IO.Compression;

namespace MeAjudaAi.ApiService.Tests.Unit.Providers;

[Trait("Category", "Unit")]
public class CompressionProvidersTests
{
    #region SafeBrolliCompressionProvider Tests
    [Fact]
    public void SafeBrotliCompressionProvider_Should_HaveCorrectProperties()
    {
        // Arrange
        var provider = new SafeBrotliCompressionProvider();

        // Assert
        provider.EncodingName.Should().Be("br");
        provider.SupportsFlush.Should().BeTrue();
    }

    [Fact]
    public void SafeBrotliCompressionProvider_CreateStream_Should_ReturnBrotliStream()
    {
        // Arrange
        var provider = new SafeBrotliCompressionProvider();
        using var memoryStream = new MemoryStream();

        // Act
        using var stream = provider.CreateStream(memoryStream);

        // Assert
        stream.Should().BeOfType<BrotliStream>();
    }

    #endregion

    #region SafeGzipCompressionProvider Tests

    [Fact]
    public void SafeGzipCompressionProvider_Should_HaveCorrectProperties()
    {
        // Arrange
        var provider = new SafeGzipCompressionProvider();

        // Assert
        provider.EncodingName.Should().Be("gzip");
        provider.SupportsFlush.Should().BeTrue();
    }

    [Fact]
    public void SafeGzipCompressionProvider_CreateStream_Should_ReturnGZipStream()
    {
        // Arrange
        var provider = new SafeGzipCompressionProvider();
        using var memoryStream = new MemoryStream();

        // Act
        using var stream = provider.CreateStream(memoryStream);

        // Assert
        stream.Should().BeOfType<GZipStream>();
    }

    #endregion
}

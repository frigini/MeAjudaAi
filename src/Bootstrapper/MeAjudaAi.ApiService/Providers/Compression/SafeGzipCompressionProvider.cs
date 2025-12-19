using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;

namespace MeAjudaAi.ApiService.Providers.Compression;

/// <summary>
/// Provedor de compressão Gzip otimizado
/// </summary>
public class SafeGzipCompressionProvider : ICompressionProvider
{
    public string EncodingName => "gzip";
    public bool SupportsFlush => true;

    public Stream CreateStream(Stream outputStream)
    {
        ArgumentNullException.ThrowIfNull(outputStream);
        return new GZipStream(outputStream, CompressionLevel.Optimal, leaveOpen: true);
    }
}

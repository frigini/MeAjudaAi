using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;

namespace MeAjudaAi.ApiService.Providers.Compression;

/// <summary>
/// Provedor de compressão Gzip seguro que previne CRIME/BREACH
/// </summary>
public class SafeGzipCompressionProvider : ICompressionProvider
{
    public string EncodingName => "gzip";
    public bool SupportsFlush => true;

    public Stream CreateStream(Stream outputStream)
    {
        return new GZipStream(outputStream, CompressionLevel.Optimal, leaveOpen: false);
    }

    public static bool ShouldCompressResponse(HttpContext context)
    {
        return Extensions.PerformanceExtensions.IsSafeForCompression(context);
    }
}

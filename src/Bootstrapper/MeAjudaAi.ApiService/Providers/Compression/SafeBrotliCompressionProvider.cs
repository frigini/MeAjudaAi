using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

namespace MeAjudaAi.ApiService.Providers.Compression;

/// <summary>
/// Provedor de compressão Brotli seguro que previne CRIME/BREACH
/// </summary>
public class SafeBrotliCompressionProvider : ICompressionProvider
{
    public string EncodingName => "br";
    public bool SupportsFlush => true;

    public Stream CreateStream(Stream outputStream)
    {
        ArgumentNullException.ThrowIfNull(outputStream);
        return new BrotliStream(outputStream, CompressionLevel.Optimal, leaveOpen: true);
    }
}

using System.IO;
using System.IO.Compression;

namespace Community.AspNetCore.RequestDecompression
{
    /// <summary>Represents GZIP decompression provider.</summary>
    public sealed class GzipDecompressionProvider : IDecompressionProvider
    {
        Stream IDecompressionProvider.CreateStream(Stream outputStream)
        {
            return new GZipStream(outputStream, CompressionMode.Decompress);
        }
    }
}
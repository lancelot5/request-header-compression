using System.IO;
using System.IO.Compression;

namespace Community.AspNetCore.RequestDecompression
{
    /// <summary>Represents Deflate decompression provider.</summary>
    public sealed class DeflateDecompressionProvider : IDecompressionProvider
    {
        Stream IDecompressionProvider.CreateStream(Stream outputStream)
        {
            return new DeflateStream(outputStream, CompressionMode.Decompress);
        }
    }
}
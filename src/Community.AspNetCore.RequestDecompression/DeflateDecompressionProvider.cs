using System.IO;
using System.IO.Compression;

namespace Community.AspNetCore.RequestDecompression
{
    /// <summary>Represents DEFLATE decompression provider.</summary>
    public sealed class DeflateDecompressionProvider : IDecompressionProvider
    {
        /// <summary>Initializes a new instance of the <see cref="DeflateDecompressionProvider" /> class.</summary>
        public DeflateDecompressionProvider()
        {
        }

        Stream IDecompressionProvider.CreateStream(Stream outputStream)
        {
            return new DeflateStream(outputStream, CompressionMode.Decompress);
        }
    }
}
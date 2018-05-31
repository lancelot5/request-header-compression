using System.IO;
using System.IO.Compression;

namespace Community.AspNetCore.RequestDecompression
{
    /// <summary>Represents Brotli decompression provider.</summary>
    internal sealed class BrotliDecompressionProvider : IDecompressionProvider
    {
        /// <summary>Initializes a new instance of the <see cref="BrotliDecompressionProvider" /> class.</summary>
        public BrotliDecompressionProvider()
        {
        }

        Stream IDecompressionProvider.CreateStream(Stream outputStream)
        {
            return new BrotliStream(outputStream, CompressionMode.Decompress);
        }

        string IDecompressionProvider.EncodingName
        {
            get => "br";
        }
    }
}
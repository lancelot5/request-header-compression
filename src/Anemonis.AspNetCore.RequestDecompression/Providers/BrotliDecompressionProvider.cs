// © Alexander Kozlenko. Licensed under the MIT License.

#if NETCOREAPP2_1

using System.IO;
using System.IO.Compression;

namespace Anemonis.AspNetCore.RequestDecompression
{
    /// <summary>Represents Brotli decompression provider.</summary>
    public sealed class BrotliDecompressionProvider : IDecompressionProvider
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

#endif
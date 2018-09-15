// © Alexander Kozlenko. Licensed under the MIT License.

using System.IO;
using System.IO.Compression;

namespace Anemonis.AspNetCore.RequestDecompression
{
    /// <summary>Represents gzip decompression provider.</summary>
    public sealed class GzipDecompressionProvider : IDecompressionProvider
    {
        /// <summary>Initializes a new instance of the <see cref="GzipDecompressionProvider" /> class.</summary>
        public GzipDecompressionProvider()
        {
        }

        Stream IDecompressionProvider.CreateStream(Stream outputStream)
        {
            return new GZipStream(outputStream, CompressionMode.Decompress);
        }

        string IDecompressionProvider.EncodingName
        {
            get => "gzip";
        }
    }
}
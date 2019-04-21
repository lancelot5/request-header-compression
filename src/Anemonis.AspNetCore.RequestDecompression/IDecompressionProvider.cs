// © Alexander Kozlenko. Licensed under the MIT License.

using System.IO;

namespace Anemonis.AspNetCore.RequestDecompression
{
    /// <summary>Defines a decompression provider.</summary>
    public interface IDecompressionProvider
    {
        /// <summary>Creates a new decompression stream.</summary>
        /// <param name="outputStream">The stream where the decompressed data have to be read from.</param>
        /// <returns>The decompression stream.</returns>
        Stream CreateStream(Stream outputStream);
    }
}
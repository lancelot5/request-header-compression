using System.IO;

namespace Community.AspNetCore.RequestDecompression
{
    /// <summary>Defines a decompression provider.</summary>
    public interface IDecompressionProvider
    {
        /// <summary>Create a new decompression stream.</summary>
        /// <param name="outputStream">The stream where the decompressed data have to be read from.</param>
        /// <returns>The decompression stream.</returns>
        Stream CreateStream(Stream outputStream);
    }
}
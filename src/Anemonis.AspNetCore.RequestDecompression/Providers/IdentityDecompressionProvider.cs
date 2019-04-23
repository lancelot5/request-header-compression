// © Alexander Kozlenko. Licensed under the MIT License.

using System.IO;

namespace Anemonis.AspNetCore.RequestDecompression
{
    internal sealed class IdentityDecompressionProvider : IDecompressionProvider
    {
        public Stream CreateStream(Stream outputStream)
        {
            return outputStream;
        }
    }
}

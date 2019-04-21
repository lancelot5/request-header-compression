using System.IO;

namespace Anemonis.AspNetCore.RequestDecompression.UnitTests.TestStubs
{
    [EncodingName("encoding")]
    internal sealed class TestDecompressionProvider10 : IDecompressionProvider
    {
        public Stream CreateStream(Stream outputStream)
        {
            return outputStream;
        }
    }
}
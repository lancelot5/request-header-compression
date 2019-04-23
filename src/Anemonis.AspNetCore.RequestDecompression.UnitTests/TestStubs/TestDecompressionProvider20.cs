using System;
using System.IO;

namespace Anemonis.AspNetCore.RequestDecompression.UnitTests.TestStubs
{
    [EncodingName("encoding")]
    internal sealed class TestDecompressionProvider20 : IDecompressionProvider
    {
        public Stream CreateStream(Stream outputStream)
        {
            throw new NotImplementedException();
        }
    }
}

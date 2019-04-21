using System;
using System.IO;

namespace Anemonis.AspNetCore.RequestDecompression.UnitTests.TestStubs
{
    internal sealed class TestDecompressionProvider00 : IDecompressionProvider
    {
        public Stream CreateStream(Stream outputStream)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.IO;

namespace Anemonis.AspNetCore.RequestDecompression.UnitTests.TestStubs
{
    internal sealed class TestDecompressionProvider2
    {
        public Stream CreateStream(Stream outputStream)
        {
            throw new NotImplementedException();
        }

        public string EncodingName
        {
            get => throw new NotImplementedException();
        }
    }
}
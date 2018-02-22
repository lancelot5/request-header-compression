using System;
using Xunit;

namespace Community.AspNetCore.RequestDecompression.Tests
{
    public sealed class RequestDecompressionOptionsTests
    {
        [Fact]
        public void RegisterWhenEncodingNameIsNull()
        {
            var options = new RequestDecompressionOptions();

            Assert.Throws<ArgumentNullException>(() =>
              options.Register<DeflateDecompressionProvider>((string)null));
        }

        [Fact]
        public void RegisterWhenEncodingNameIsIdentity()
        {
            var options = new RequestDecompressionOptions();

            Assert.Throws<ArgumentException>(() =>
                options.Register<DeflateDecompressionProvider>("identity"));
        }
    }
}
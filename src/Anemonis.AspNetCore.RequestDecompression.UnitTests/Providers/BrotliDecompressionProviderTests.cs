using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Anemonis.AspNetCore.RequestDecompression.UnitTests.Providers
{
    [TestClass]
    public sealed class BrotliDecompressionProviderTests
    {
        [TestMethod]
        public void CreateStream()
        {
            var provider = new BrotliDecompressionProvider() as IDecompressionProvider;
            var stream = provider.CreateStream(new MemoryStream());

            Assert.IsNotNull(stream);
        }

        [TestMethod]
        public void EncodingName()
        {
            var provider = new BrotliDecompressionProvider() as IDecompressionProvider;

            Assert.IsNotNull(provider.EncodingName);
        }
    }
}
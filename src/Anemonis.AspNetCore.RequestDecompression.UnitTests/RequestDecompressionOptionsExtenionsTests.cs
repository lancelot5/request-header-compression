using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Anemonis.AspNetCore.RequestDecompression.UnitTests
{
    [TestClass]
    public sealed class RequestDecompressionOptionsExtenionsTests
    {
        [TestMethod]
        public void UseDefaultsWithOptionsWhenOptionsIsNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                RequestDecompressionOptionsExtenions.UseDefaults(null));
        }

        [TestMethod]
        public void UseDefaultsWithOptions()
        {
            var options = new RequestDecompressionOptions();

            RequestDecompressionOptionsExtenions.UseDefaults(options);

            Assert.IsNotNull(options.Providers);
            Assert.AreEqual(3, options.Providers.Count);
            Assert.IsTrue(options.Providers.Contains(typeof(BrotliDecompressionProvider)));
            Assert.IsTrue(options.Providers.Contains(typeof(DeflateDecompressionProvider)));
            Assert.IsTrue(options.Providers.Contains(typeof(GzipDecompressionProvider)));
        }
    }
}
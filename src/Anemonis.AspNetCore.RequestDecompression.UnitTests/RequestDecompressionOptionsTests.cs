using System;
using System.Linq;
using Anemonis.AspNetCore.RequestDecompression.UnitTests.TestStubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Anemonis.AspNetCore.RequestDecompression.UnitTests
{
    [TestClass]
    public sealed class RequestDecompressionOptionsTests
    {
        [TestMethod]
        public void Constructor()
        {
            var options = new RequestDecompressionOptions();

            Assert.IsTrue(options.SkipUnsupportedEncodings);
            Assert.IsNotNull(options.Providers);
            Assert.AreEqual(0, options.Providers.Count);
        }

        [TestMethod]
        public void AddProvider()
        {
            var options = new RequestDecompressionOptions();

            options.AddProvider<TestDecompressionProvider1>();

            Assert.IsNotNull(options.Providers);
            Assert.AreEqual(1, options.Providers.Count);
            Assert.IsTrue(options.Providers.Contains(typeof(TestDecompressionProvider1)));
        }

        [TestMethod]
        public void AddProviderWhenCalledTimes()
        {
            var options = new RequestDecompressionOptions();

            options.AddProvider<TestDecompressionProvider1>();
            options.AddProvider<TestDecompressionProvider1>();

            Assert.IsNotNull(options.Providers);
            Assert.AreEqual(1, options.Providers.Count);
            Assert.IsTrue(options.Providers.Contains(typeof(TestDecompressionProvider1)));
        }

        [TestMethod]
        public void AddProviderWithTypeWhenTypeIsNull()
        {
            var options = new RequestDecompressionOptions();

            Assert.ThrowsException<ArgumentNullException>(() =>
                options.AddProvider(null));
        }

        [TestMethod]
        public void AddProviderWithTypeWhenTypeDoesNotImplementInterface()
        {
            var options = new RequestDecompressionOptions();

            Assert.ThrowsException<ArgumentException>(() =>
                options.AddProvider(typeof(TestDecompressionProvider2)));
        }

        [TestMethod]
        public void AddProviderWithType()
        {
            var options = new RequestDecompressionOptions();

            options.AddProvider(typeof(TestDecompressionProvider1));

            Assert.IsNotNull(options.Providers);
            Assert.AreEqual(1, options.Providers.Count);
            Assert.IsTrue(options.Providers.Contains(typeof(TestDecompressionProvider1)));
        }

        [TestMethod]
        public void AddProviderWithTypeWhenCalledTimes()
        {
            var options = new RequestDecompressionOptions();

            options.AddProvider(typeof(TestDecompressionProvider1));
            options.AddProvider(typeof(TestDecompressionProvider1));

            Assert.IsNotNull(options.Providers);
            Assert.AreEqual(1, options.Providers.Count);
            Assert.IsTrue(options.Providers.Contains(typeof(TestDecompressionProvider1)));
        }
    }
}
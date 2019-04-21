using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Anemonis.AspNetCore.RequestDecompression.UnitTests
{
    [TestClass]
    public sealed class RequestDecompressionServicesExtensionsTests
    {
        [TestMethod]
        public void AddRequestDecompressionWithServicesWhenServicesIsNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                RequestDecompressionServicesExtensions.AddRequestDecompression(null));
        }

        [TestMethod]
        public void AddRequestDecompressionWithServicesAndOptionsWhenServicesIsNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                RequestDecompressionServicesExtensions.AddRequestDecompression(null, co => { }));
        }

        [TestMethod]
        public void AddRequestDecompressionWithServicesAndOptionsWhenOptionsIsNull()
        {
            var servicesMock = new Mock<IServiceCollection>(MockBehavior.Strict);

            Assert.ThrowsException<ArgumentNullException>(() =>
                RequestDecompressionServicesExtensions.AddRequestDecompression(servicesMock.Object, null));
        }

        [TestMethod]
        public void AddRequestDecompressionWithServicesAndOptions()
        {
            var servicesMock = new Mock<IServiceCollection>(MockBehavior.Strict);

            servicesMock.Setup(o => o.GetEnumerator())
                .Returns(new List<ServiceDescriptor>().GetEnumerator());
            servicesMock.Setup(o => o.Add(It.IsNotNull<ServiceDescriptor>()));

            RequestDecompressionServicesExtensions.AddRequestDecompression(servicesMock.Object, co => { });
        }
    }
}
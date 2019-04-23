using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Anemonis.AspNetCore.RequestDecompression.UnitTests
{
    [TestClass]
    public sealed class RequestDecompressionBuilderExtensionsTests
    {
        [TestMethod]
        public void UseRequestDecompressionWhenBuilderIsNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                RequestDecompressionBuilderExtensions.UseRequestDecompression(null));
        }

        [TestMethod]
        public void UseRequestDecompression()
        {
            var builderMock = new Mock<IApplicationBuilder>(MockBehavior.Strict);

            builderMock
                .Setup(o => o.New())
                .Returns(builderMock.Object);
            builderMock
                .Setup(o => o.Use(It.IsNotNull<Func<RequestDelegate, RequestDelegate>>()))
                .Returns(builderMock.Object);
            builderMock
                .Setup(o => o.Build())
                .Returns(c => Task.CompletedTask);

            RequestDecompressionBuilderExtensions.UseRequestDecompression(builderMock.Object);

            builderMock.Verify(o => o.Use(It.IsNotNull<Func<RequestDelegate, RequestDelegate>>()), Times.AtLeastOnce());
        }
    }
}

using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Community.AspNetCore.RequestDecompression.IntegrationTests.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Community.AspNetCore.RequestDecompression.IntegrationTests
{
    [TestClass]
    public sealed class RequestDecompressionMiddlewareTests
    {
        private static void TestActionForDecodedContent(HttpRequest request)
        {
            Assert.IsFalse(request.Headers.ContainsKey(HeaderNames.ContentEncoding));
            Assert.IsTrue(request.Headers.ContainsKey(HeaderNames.ContentLength));

            var content = default(byte[]);

            using (var buffer = new MemoryStream())
            {
                request.Body.CopyTo(buffer);
                content = buffer.ToArray();
            }

            Assert.AreEqual(content.Length.ToString(CultureInfo.InvariantCulture), request.Headers[HeaderNames.ContentLength].ToString());

            CollectionAssert.AreEqual(CreateContentSample(), content);
        }

        private void TestActionForPartiallyDecodedContent(HttpRequest request)
        {
            Assert.IsTrue(request.Headers.ContainsKey(HeaderNames.ContentEncoding));
            Assert.AreEqual((StringValues)new[] { "identity", "unknown" }, request.Headers[HeaderNames.ContentEncoding]);
        }

        private void TestActionForUndecodedContent(HttpRequest request)
        {
            Assert.IsTrue(request.Headers.ContainsKey(HeaderNames.ContentEncoding));
            Assert.AreEqual((StringValues)"unknown", request.Headers[HeaderNames.ContentEncoding]);
        }

        [TestMethod]
        public async Task HandleEmptyEncoding()
        {
            var options = new RequestDecompressionOptions();

            options.Register<DeflateDecompressionProvider>();
            options.Register<GzipDecompressionProvider>();

            var builder = new WebHostBuilder()
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options)
                    .AddRequestTest(TestActionForDecodedContent))
                .Configure(ab => ab
                    .UseRequestDecompression()
                    .UseRequestTest());

            using (var server = new TestServer(builder))
            {
                using (var client = server.CreateClient())
                {
                    var contentBytes = CreateContentSample();
                    var requestContent = new ByteArrayContent(contentBytes);

                    requestContent.Headers.ContentLength = contentBytes.Length;

                    await client.PostAsync(server.BaseAddress, requestContent);
                }
            }
        }

        [TestMethod]
        public async Task HandleIdentityEncoding()
        {
            var options = new RequestDecompressionOptions();

            options.Register<DeflateDecompressionProvider>();
            options.Register<GzipDecompressionProvider>();

            var builder = new WebHostBuilder()
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options)
                    .AddRequestTest(TestActionForDecodedContent))
                .Configure(ab => ab
                    .UseRequestDecompression()
                    .UseRequestTest());

            using (var server = new TestServer(builder))
            {
                using (var client = server.CreateClient())
                {
                    var contentBytes = CreateContentSample();
                    var requestContent = new ByteArrayContent(contentBytes);

                    requestContent.Headers.ContentEncoding.Add("identity");

                    await client.PostAsync(server.BaseAddress, requestContent);
                }
            }
        }

        [TestMethod]
        public async Task HandleDeflateEncoding()
        {
            var options = new RequestDecompressionOptions();

            options.Register<DeflateDecompressionProvider>();

            var builder = new WebHostBuilder()
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options)
                    .AddRequestTest(TestActionForDecodedContent))
                .Configure(ab => ab
                    .UseRequestDecompression()
                    .UseRequestTest());

            using (var server = new TestServer(builder))
            {
                using (var client = server.CreateClient())
                {
                    var requestContent = new ByteArrayContent(CompressWithDeflate(CreateContentSample()));

                    requestContent.Headers.ContentEncoding.Add("deflate");

                    await client.PostAsync(server.BaseAddress, requestContent);
                }
            }
        }

        [TestMethod]
        public async Task HandleGzipEncoding()
        {
            var options = new RequestDecompressionOptions();

            options.Register<GzipDecompressionProvider>();

            var builder = new WebHostBuilder()
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options)
                    .AddRequestTest(TestActionForDecodedContent))
                .Configure(ab => ab
                    .UseRequestDecompression()
                    .UseRequestTest());

            using (var server = new TestServer(builder))
            {
                using (var client = server.CreateClient())
                {
                    var requestContent = new ByteArrayContent(CompressWithGzip(CreateContentSample()));

                    requestContent.Headers.ContentEncoding.Add("gzip");

                    await client.PostAsync(server.BaseAddress, requestContent);
                }
            }
        }

        [TestMethod]
        public async Task HandleBrotliEncoding()
        {
            var options = new RequestDecompressionOptions();

            options.Register<BrotliDecompressionProvider>();

            var builder = new WebHostBuilder()
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options)
                    .AddRequestTest(TestActionForDecodedContent))
                .Configure(ab => ab
                    .UseRequestDecompression()
                    .UseRequestTest());

            using (var server = new TestServer(builder))
            {
                using (var client = server.CreateClient())
                {
                    var requestContent = new ByteArrayContent(CompressWithBrotli(CreateContentSample()));

                    requestContent.Headers.ContentEncoding.Add("br");

                    await client.PostAsync(server.BaseAddress, requestContent);
                }
            }
        }

        [TestMethod]
        public async Task HandleMultipleEncodings()
        {
            var options = new RequestDecompressionOptions();

            options.UseDefaults();

            var builder = new WebHostBuilder()
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options)
                    .AddRequestTest(TestActionForDecodedContent))
                .Configure(ab => ab
                    .UseRequestDecompression()
                    .UseRequestTest());

            using (var server = new TestServer(builder))
            {
                using (var client = server.CreateClient())
                {
                    var requestContent = new ByteArrayContent(CompressWithBrotli(CompressWithGzip(CompressWithDeflate(CreateContentSample()))));

                    requestContent.Headers.ContentEncoding.Add("identity");
                    requestContent.Headers.ContentEncoding.Add("deflate");
                    requestContent.Headers.ContentEncoding.Add("gzip");
                    requestContent.Headers.ContentEncoding.Add("br");

                    await client.PostAsync(server.BaseAddress, requestContent);
                }
            }
        }

        [TestMethod]
        public async Task HandleMultipleEncodingsWithUknown()
        {
            var options = new RequestDecompressionOptions();

            options.UseDefaults();

            var builder = new WebHostBuilder()
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options)
                    .AddRequestTest(TestActionForPartiallyDecodedContent))
                .Configure(ab => ab
                    .UseRequestDecompression()
                    .UseRequestTest());

            using (var server = new TestServer(builder))
            {
                using (var client = server.CreateClient())
                {
                    var requestContent = new ByteArrayContent(CompressWithBrotli(CompressWithGzip(CompressWithDeflate(CreateContentSample()))));

                    requestContent.Headers.ContentEncoding.Add("identity");
                    requestContent.Headers.ContentEncoding.Add("unknown");
                    requestContent.Headers.ContentEncoding.Add("deflate");
                    requestContent.Headers.ContentEncoding.Add("gzip");
                    requestContent.Headers.ContentEncoding.Add("br");

                    await client.PostAsync(server.BaseAddress, requestContent);
                }
            }
        }

        [TestMethod]
        public async Task HandlUnknownEncoding()
        {
            var options = new RequestDecompressionOptions();

            var builder = new WebHostBuilder()
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options)
                    .AddRequestTest(TestActionForUndecodedContent))
                .Configure(ab => ab
                    .UseRequestDecompression()
                    .UseRequestTest());

            using (var server = new TestServer(builder))
            {
                using (var client = server.CreateClient())
                {
                    var requestContent = new ByteArrayContent(CreateContentSample());

                    requestContent.Headers.ContentEncoding.Add("unknown");

                    await client.PostAsync(server.BaseAddress, requestContent);
                }
            }
        }

        [TestMethod]
        public async Task HandlUnknownEncodingWithReject()
        {
            var options = new RequestDecompressionOptions()
            {
                SkipUnsupportedEncodings = false
            };

            var builder = new WebHostBuilder()
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options)
                    .AddRequestTest(TestActionForUndecodedContent))
                .Configure(ab => ab
                    .UseRequestDecompression()
                    .UseRequestTest());

            using (var server = new TestServer(builder))
            {
                using (var client = server.CreateClient())
                {
                    var requestContent = new ByteArrayContent(CreateContentSample());

                    requestContent.Headers.ContentEncoding.Add("unknown");

                    var response = await client.PostAsync(server.BaseAddress, requestContent);

                    Assert.AreEqual(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
                }
            }
        }

        private static byte[] CompressWithDeflate(byte[] content)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var compressionStream = new DeflateStream(outputStream, CompressionLevel.Optimal))
                {
                    compressionStream.Write(content, 0, content.Length);
                }

                return outputStream.ToArray();
            }
        }

        private static byte[] CompressWithGzip(byte[] content)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var compressionStream = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    compressionStream.Write(content, 0, content.Length);
                }

                return outputStream.ToArray();
            }
        }

        private static byte[] CompressWithBrotli(byte[] content)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var compressionStream = new BrotliStream(outputStream, CompressionLevel.Optimal))
                {
                    compressionStream.Write(content, 0, content.Length);
                }

                return outputStream.ToArray();
            }
        }

        private static byte[] CreateContentSample()
        {
            var decodedContent = new byte[byte.MaxValue];

            for (var i = 0; i < decodedContent.Length; i++)
            {
                decodedContent[i] = (byte)i;
            }

            return decodedContent;
        }
    }
}
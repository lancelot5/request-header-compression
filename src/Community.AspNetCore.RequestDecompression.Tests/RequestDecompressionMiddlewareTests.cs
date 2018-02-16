using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using Community.AspNetCore.RequestDecompression.Tests.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Xunit;
using Xunit.Abstractions;

namespace Community.AspNetCore.RequestDecompression.Tests
{
    public sealed class RequestDecompressionMiddlewareTests
    {
        private readonly ITestOutputHelper _output;

        public RequestDecompressionMiddlewareTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private static void TestActionForDecodedContent(HttpRequest request)
        {
            Assert.False(request.Headers.ContainsKey(HeaderNames.ContentEncoding));
            Assert.True(request.Headers.ContainsKey(HeaderNames.ContentLength));

            var content = default(byte[]);

            using (var buffer = new MemoryStream())
            {
                request.Body.CopyTo(buffer);
                content = buffer.ToArray();
            }

            Assert.Equal(content.Length.ToString(CultureInfo.InvariantCulture), request.Headers[HeaderNames.ContentLength]);
            Assert.Equal(CreateContentSample(), content);
        }

        private void TestActionForPartiallyDecodedContent(HttpRequest request)
        {
            Assert.True(request.Headers.ContainsKey(HeaderNames.ContentEncoding));
            Assert.Equal(new StringValues(new[] { "identity", "unknown" }), request.Headers[HeaderNames.ContentEncoding]);
        }

        private void TestActionForUndecodedContent(HttpRequest request)
        {
            Assert.True(request.Headers.ContainsKey(HeaderNames.ContentEncoding));
            Assert.Equal(new StringValues("unknown"), request.Headers[HeaderNames.ContentEncoding]);
        }

        [Fact]
        public async void HandleEmptyEncoding()
        {
            var options = new RequestDecompressionOptions();

            options.Register<DeflateDecompressionProvider>("deflate");
            options.Register<GzipDecompressionProvider>("gzip");

            var builder = new WebHostBuilder()
                .ConfigureLogging(lb => lb
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddXunit(_output))
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options)
                    .AddRequestTest(TestActionForDecodedContent))
                .Configure(ab => ab
                    .UseResponseCompression()
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

        [Fact]
        public async void HandleIdentityEncoding()
        {
            var options = new RequestDecompressionOptions();

            options.Register<DeflateDecompressionProvider>("deflate");
            options.Register<GzipDecompressionProvider>("gzip");

            var builder = new WebHostBuilder()
                .ConfigureLogging(lb => lb
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddXunit(_output))
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options)
                    .AddRequestTest(TestActionForDecodedContent))
                .Configure(ab => ab
                    .UseResponseCompression()
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

        [Fact]
        public async void HandleDeflateEncoding()
        {
            var options = new RequestDecompressionOptions();

            options.Register<DeflateDecompressionProvider>("deflate");

            var builder = new WebHostBuilder()
                .ConfigureLogging(lb => lb
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddXunit(_output))
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options)
                    .AddRequestTest(TestActionForDecodedContent))
                .Configure(ab => ab
                    .UseResponseCompression()
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

        [Fact]
        public async void HandleGzipEncoding()
        {
            var options = new RequestDecompressionOptions();

            options.Register<GzipDecompressionProvider>("gzip");

            var builder = new WebHostBuilder()
                .ConfigureLogging(lb => lb
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddXunit(_output))
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options)
                    .AddRequestTest(TestActionForDecodedContent))
                .Configure(ab => ab
                    .UseResponseCompression()
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

        [Fact]
        public async void HandleMultipleEncodings()
        {
            var options = new RequestDecompressionOptions();

            options.Register<DeflateDecompressionProvider>("deflate");
            options.Register<GzipDecompressionProvider>("gzip");

            var builder = new WebHostBuilder()
                .ConfigureLogging(lb => lb
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddXunit(_output))
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options)
                    .AddRequestTest(TestActionForDecodedContent))
                .Configure(ab => ab
                    .UseResponseCompression()
                    .UseRequestTest());

            using (var server = new TestServer(builder))
            {
                using (var client = server.CreateClient())
                {
                    var requestContent = new ByteArrayContent(CompressWithGzip(CompressWithDeflate(CreateContentSample())));

                    requestContent.Headers.ContentEncoding.Add("identity");
                    requestContent.Headers.ContentEncoding.Add("deflate");
                    requestContent.Headers.ContentEncoding.Add("gzip");

                    await client.PostAsync(server.BaseAddress, requestContent);
                }
            }
        }

        [Fact]
        public async void HandleMultipleEncodingsWithUknown()
        {
            var options = new RequestDecompressionOptions();

            options.Register<DeflateDecompressionProvider>("deflate");
            options.Register<GzipDecompressionProvider>("gzip");

            var builder = new WebHostBuilder()
                .ConfigureLogging(lb => lb
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddXunit(_output))
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options)
                    .AddRequestTest(TestActionForPartiallyDecodedContent))
                .Configure(ab => ab
                    .UseResponseCompression()
                    .UseRequestTest());

            using (var server = new TestServer(builder))
            {
                using (var client = server.CreateClient())
                {
                    var requestContent = new ByteArrayContent(CompressWithGzip(CompressWithDeflate(CreateContentSample())));

                    requestContent.Headers.ContentEncoding.Add("identity");
                    requestContent.Headers.ContentEncoding.Add("unknown");
                    requestContent.Headers.ContentEncoding.Add("deflate");
                    requestContent.Headers.ContentEncoding.Add("gzip");

                    await client.PostAsync(server.BaseAddress, requestContent);
                }
            }
        }

        [Fact]
        public async void HandlUnknownEncoding()
        {
            var options = new RequestDecompressionOptions();

            var builder = new WebHostBuilder()
                .ConfigureLogging(lb => lb
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddXunit(_output))
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options)
                    .AddRequestTest(TestActionForUndecodedContent))
                .Configure(ab => ab
                    .UseResponseCompression()
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

        [Fact]
        public async void HandlUnknownEncodingWithReject()
        {
            var options = new RequestDecompressionOptions()
            {
                SkipUnsupportedEncodings = false
            };

            var builder = new WebHostBuilder()
                .ConfigureLogging(lb => lb
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddXunit(_output))
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options)
                    .AddRequestTest(TestActionForUndecodedContent))
                .Configure(ab => ab
                    .UseResponseCompression()
                    .UseRequestTest());

            using (var server = new TestServer(builder))
            {
                using (var client = server.CreateClient())
                {
                    var requestContent = new ByteArrayContent(CreateContentSample());

                    requestContent.Headers.ContentEncoding.Add("unknown");

                    var response = await client.PostAsync(server.BaseAddress, requestContent);

                    Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
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
                    compressionStream.Close();
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
                    compressionStream.Close();
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
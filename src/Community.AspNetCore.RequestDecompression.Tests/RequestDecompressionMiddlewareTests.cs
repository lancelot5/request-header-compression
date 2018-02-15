using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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

        private static async Task TestActionForDecodedContent(HttpRequest request)
        {
            var expected = "0123456789";

            Assert.False(request.Headers.ContainsKey(HeaderNames.ContentEncoding));
            Assert.True(request.Headers.ContainsKey(HeaderNames.ContentLength));
            Assert.Equal(expected.Length.ToString(CultureInfo.InvariantCulture), request.Headers[HeaderNames.ContentLength]);

            var actual = default(string);

            using (var reader = new StreamReader(request.Body, Encoding.UTF8))
            {
                actual = await reader.ReadToEndAsync().ConfigureAwait(false);
            }

            Assert.Equal(expected, actual);
        }

        private static Task TestActionForPartiallyDecodedContent(HttpRequest request)
        {
            Assert.True(request.Headers.ContainsKey(HeaderNames.ContentEncoding));
            Assert.Equal(new StringValues(new[] { "identity", "unknown" }), request.Headers[HeaderNames.ContentEncoding]);

            return Task.CompletedTask;
        }

        private static Task TestActionForUndecodedContent(HttpRequest request)
        {
            Assert.True(request.Headers.ContainsKey(HeaderNames.ContentEncoding));
            Assert.Equal(new StringValues("unknown"), request.Headers[HeaderNames.ContentEncoding]);

            return Task.CompletedTask;
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
                    var contentBytes = Encoding.UTF8.GetBytes("0123456789");
                    var requestContent = new ByteArrayContent(contentBytes);

                    requestContent.Headers.ContentLength = contentBytes.Length;

                    await client.PostAsync(server.BaseAddress, requestContent).ConfigureAwait(false);
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
                    var contentBytes = Encoding.UTF8.GetBytes("0123456789");
                    var requestContent = new ByteArrayContent(contentBytes);

                    requestContent.Headers.ContentEncoding.Add("identity");

                    await client.PostAsync(server.BaseAddress, requestContent).ConfigureAwait(false);
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
                    var requestContent = new ByteArrayContent(CompressWithDeflate(Encoding.UTF8.GetBytes("0123456789")));

                    requestContent.Headers.ContentEncoding.Add("deflate");

                    await client.PostAsync(server.BaseAddress, requestContent).ConfigureAwait(false);
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
                    var requestContent = new ByteArrayContent(CompressWithGzip(Encoding.UTF8.GetBytes("0123456789")));

                    requestContent.Headers.ContentEncoding.Add("gzip");

                    await client.PostAsync(server.BaseAddress, requestContent).ConfigureAwait(false);
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
                    var requestContent = new ByteArrayContent(CompressWithGzip(CompressWithDeflate(Encoding.UTF8.GetBytes("0123456789"))));

                    requestContent.Headers.ContentEncoding.Add("identity");
                    requestContent.Headers.ContentEncoding.Add("deflate");
                    requestContent.Headers.ContentEncoding.Add("gzip");

                    await client.PostAsync(server.BaseAddress, requestContent).ConfigureAwait(false);
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
                    var requestContent = new ByteArrayContent(CompressWithGzip(CompressWithDeflate(Encoding.UTF8.GetBytes("0123456789"))));

                    requestContent.Headers.ContentEncoding.Add("identity");
                    requestContent.Headers.ContentEncoding.Add("unknown");
                    requestContent.Headers.ContentEncoding.Add("deflate");
                    requestContent.Headers.ContentEncoding.Add("gzip");

                    await client.PostAsync(server.BaseAddress, requestContent).ConfigureAwait(false);
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
                    var requestContent = new ByteArrayContent(Encoding.UTF8.GetBytes("0123456789"));

                    requestContent.Headers.ContentEncoding.Add("unknown");

                    await client.PostAsync(server.BaseAddress, requestContent).ConfigureAwait(false);
                }
            }
        }

        [Fact]
        public async void HandlUnknownEncodingWithReject()
        {
            var options = new RequestDecompressionOptions()
            {
                RejectUnsupported = true
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
                    var requestContent = new ByteArrayContent(Encoding.UTF8.GetBytes("0123456789"));

                    requestContent.Headers.ContentEncoding.Add("unknown");

                    var response = await client.PostAsync(server.BaseAddress, requestContent).ConfigureAwait(false);

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
    }
}
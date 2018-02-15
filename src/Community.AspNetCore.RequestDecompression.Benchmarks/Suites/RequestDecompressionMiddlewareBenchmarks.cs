using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Community.AspNetCore.RequestDecompression.Benchmarks.Framework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Community.AspNetCore.RequestDecompression.Benchmarks.Suites
{
    [BenchmarkSuite("RequestDecompressionMiddleware")]
    public abstract class RequestDecompressionMiddlewareBenchmarks
    {
        private static readonly IReadOnlyDictionary<string, byte[]> _contents;

        private readonly TestServer _server;
        private readonly HttpClient _client;

        static RequestDecompressionMiddlewareBenchmarks()
        {
            var decodedContent = new byte[byte.MaxValue];

            for (var i = 0; i < decodedContent.Length; i++)
            {
                decodedContent[i] = (byte)i;
            }

            var contents = new Dictionary<string, byte[]>(StringComparer.Ordinal)
            {
                [""] = decodedContent,
                ["identity"] = decodedContent,
                ["unknown"] = decodedContent
            };

            using (var outputStream = new MemoryStream())
            {
                using (var compressionStream = new DeflateStream(outputStream, CompressionLevel.Optimal))
                {
                    compressionStream.Write(decodedContent, 0, decodedContent.Length);
                    compressionStream.Close();
                }

                contents["deflate"] = outputStream.ToArray();
            }
            using (var outputStream = new MemoryStream())
            {
                using (var compressionStream = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    compressionStream.Write(decodedContent, 0, decodedContent.Length);
                    compressionStream.Close();
                }

                contents["gzip"] = outputStream.ToArray();
            }

            _contents = contents;
        }

        protected RequestDecompressionMiddlewareBenchmarks()
        {
            var options = new RequestDecompressionOptions();

            options.Register<DeflateDecompressionProvider>("deflate");
            options.Register<GzipDecompressionProvider>("gzip");

            var builder = new WebHostBuilder()
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options))
                .Configure(ab => ab
                    .UseResponseCompression());

            _server = new TestServer(builder);
            _client = _server.CreateClient();
        }

        private static HttpContent CreateHttpContent(string encodingName)
        {
            var result = new ByteArrayContent(_contents[encodingName]);

            if (!string.IsNullOrEmpty(encodingName))
            {
                result.Headers.ContentEncoding.Add(encodingName);
            }

            return result;
        }

        [Benchmark(Description = "empt", Baseline = true)]
        public async Task DecompressEmptyEncoding()
        {
            await _client.PostAsync(_server.BaseAddress, CreateHttpContent(""));
        }

        [Benchmark(Description = "idty")]
        public async Task DecompressIdentityEncoding()
        {
            await _client.PostAsync(_server.BaseAddress, CreateHttpContent("identity"));
        }

        [Benchmark(Description = "defl")]
        public async Task DecompressDeflateEncoding()
        {
            await _client.PostAsync(_server.BaseAddress, CreateHttpContent("deflate"));
        }

        [Benchmark(Description = "gzip")]
        public async Task DecompressGzipEncoding()
        {
            await _client.PostAsync(_server.BaseAddress, CreateHttpContent("gzip"));
        }

        [Benchmark(Description = "unkn")]
        public async Task DecompressUnknownEncoding()
        {
            await _client.PostAsync(_server.BaseAddress, CreateHttpContent("unknown"));
        }
    }
}
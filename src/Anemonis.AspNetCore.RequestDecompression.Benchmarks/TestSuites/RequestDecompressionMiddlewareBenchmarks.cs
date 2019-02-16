using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Anemonis.AspNetCore.RequestDecompression.Benchmarks.TestSuites
{
    public class RequestDecompressionMiddlewareBenchmarks
    {
        private static readonly IReadOnlyDictionary<string, byte[]> _resources = CreateResourceDictionary();

        private readonly TestServer _server;
        private readonly HttpClient _client;

        private static IReadOnlyDictionary<string, byte[]> CreateResourceDictionary()
        {
            var decodedContent = new byte[byte.MaxValue];

            for (var i = 0; i < decodedContent.Length; i++)
            {
                decodedContent[i] = (byte)i;
            }

            var resources = new Dictionary<string, byte[]>(StringComparer.Ordinal)
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
                }

                resources["deflate"] = outputStream.ToArray();
            }
            using (var outputStream = new MemoryStream())
            {
                using (var compressionStream = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    compressionStream.Write(decodedContent, 0, decodedContent.Length);
                }

                resources["gzip"] = outputStream.ToArray();
            }

            return resources;
        }

        public RequestDecompressionMiddlewareBenchmarks()
        {
            var options = new RequestDecompressionOptions();

            options.AddProvider<DeflateDecompressionProvider>();
            options.AddProvider<GzipDecompressionProvider>();

            var builder = new WebHostBuilder()
                .ConfigureServices(sc => sc
                    .AddRequestDecompression(options))
                .Configure(ab => ab
                    .UseRequestDecompression());

            _server = new TestServer(builder);
            _client = _server.CreateClient();
        }

        private static HttpContent CreateHttpContent(string encodingName)
        {
            var result = new ByteArrayContent(_resources[encodingName]);

            if (!string.IsNullOrEmpty(encodingName))
            {
                result.Headers.ContentEncoding.Add(encodingName);
            }

            return result;
        }

        [Benchmark(Description = "Content-Encoding=         ", Baseline = true)]
        public async Task<object> DecompressEmptyEncoding()
        {
            return await _client.PostAsync(_server.BaseAddress, CreateHttpContent(""));
        }

        [Benchmark(Description = "Content-Encoding=identity ")]
        public async Task<object> DecompressIdentityEncoding()
        {
            return await _client.PostAsync(_server.BaseAddress, CreateHttpContent("identity"));
        }

        [Benchmark(Description = "Content-Encoding=deflate  ")]
        public async Task<object> DecompressDeflateEncoding()
        {
            return await _client.PostAsync(_server.BaseAddress, CreateHttpContent("deflate"));
        }

        [Benchmark(Description = "Content-Encoding=gzip     ")]
        public async Task<object> DecompressGzipEncoding()
        {
            return await _client.PostAsync(_server.BaseAddress, CreateHttpContent("gzip"));
        }

        [Benchmark(Description = "Content-Encoding=unknown"  )]
        public async Task<object> DecompressUnknownEncoding()
        {
            return await _client.PostAsync(_server.BaseAddress, CreateHttpContent("unknown"));
        }
    }
}
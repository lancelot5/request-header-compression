using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Anemonis.AspNetCore.RequestDecompression.Benchmarks.TestSuites
{
    public partial class RequestDecompressionMiddlewareBenchmarks
    {
        private static readonly IReadOnlyDictionary<string, (string Encoding, byte[] Content)> _resources = CreateResourceDictionary();

        private readonly IMiddleware _middleware = CreateMiddleware();

        private static IMiddleware CreateMiddleware()
        {
            var webHost = new WebHostBuilder()
                .ConfigureServices(sc => sc.AddRequestDecompression(co =>
                    {
                        co.Providers.Add<DeflateDecompressionProvider>();
                        co.Providers.Add<GzipDecompressionProvider>();
                        co.Providers.Add<BrotliDecompressionProvider>();
                    }))
                .Configure(ab => ab.UseRequestDecompression())
                .Build();

            return (IMiddleware)webHost.Services.GetService(typeof(RequestDecompressionMiddleware));
        }

        private static IReadOnlyDictionary<string, (string, byte[])> CreateResourceDictionary()
        {
            var content = Encoding.UTF8.GetBytes("Hello World!");

            var resources = new Dictionary<string, (string, byte[])>(StringComparer.Ordinal)
            {
                ["ne"] = ("", content),
                ["id"] = ("identity", content),
                ["df"] = ("deflate", CompressionEncoder.Encode(content, "deflate")),
                ["gz"] = ("gzip", CompressionEncoder.Encode(content, "gzip")),
                ["br"] = ("br", CompressionEncoder.Encode(content, "br")),
                ["un"] = ("unknown", CompressionEncoder.Encode(content, "unknown"))
            };

            return resources;
        }

        private static HttpContext CreateHttpContext(string name)
        {
            var (encoding, content) = _resources[name];
            var result = new DefaultHttpContext();

            result.Request.Method = HttpMethods.Post;
            result.Request.Headers.Add(HeaderNames.ContentEncoding, encoding);
            result.Request.Body = new MemoryStream(content, false);

            return result;
        }

        private static Task FinishInvokeChain(HttpContext context)
        {
            return Task.CompletedTask;
        }

        [Benchmark(Description = "ENCODING=NE", Baseline = true)]
        public async Task DecompressNE()
        {
            await _middleware.InvokeAsync(CreateHttpContext("ne"), FinishInvokeChain).ConfigureAwait(false);
        }

        [Benchmark(Description = "ENCODING=ID")]
        public async Task DecompressID()
        {
            await _middleware.InvokeAsync(CreateHttpContext("id"), FinishInvokeChain).ConfigureAwait(false);
        }

        [Benchmark(Description = "ENCODING=DF")]
        public async Task DecompressDF()
        {
            await _middleware.InvokeAsync(CreateHttpContext("df"), FinishInvokeChain).ConfigureAwait(false);
        }

        [Benchmark(Description = "ENCODING=GZ")]
        public async Task DecompressGZ()
        {
            await _middleware.InvokeAsync(CreateHttpContext("gz"), FinishInvokeChain).ConfigureAwait(false);
        }

        [Benchmark(Description = "ENCODING=BR")]
        public async Task DecompressBR()
        {
            await _middleware.InvokeAsync(CreateHttpContext("br"), FinishInvokeChain).ConfigureAwait(false);
        }

        [Benchmark(Description = "ENCODING=UN")]
        public async Task DecompressUN()
        {
            await _middleware.InvokeAsync(CreateHttpContext("un"), FinishInvokeChain).ConfigureAwait(false);
        }
    }
}
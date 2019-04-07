using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Anemonis.AspNetCore.RequestDecompression.Benchmarks.TestSuites
{
    public class RequestDecompressionMiddlewareBenchmarks
    {
        private static readonly IReadOnlyDictionary<string, (string Encoding, byte[] Content)> _resources = CreateResourceDictionary();

        private readonly IMiddleware _middleware = CreateMiddleware();

        private static IMiddleware CreateMiddleware()
        {
            var options = new RequestDecompressionOptions();

            options.AddProvider<DeflateDecompressionProvider>();
            options.AddProvider<GzipDecompressionProvider>();
            options.AddProvider<BrotliDecompressionProvider>();

            var webHost = new WebHostBuilder()
                .ConfigureServices(sc => sc.AddRequestDecompression(options))
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

        private static class CompressionEncoder
        {
            public static byte[] Encode(byte[] content, string algorithm)
            {
                switch (algorithm)
                {
                    case "identity":
                        {
                            return content;
                        }
                    case "deflate":
                        {
                            using (var contentStream = new MemoryStream())
                            {
                                using (var compressionStream = new DeflateStream(contentStream, CompressionLevel.Optimal))
                                {
                                    compressionStream.Write(content, 0, content.Length);
                                }

                                return contentStream.ToArray();
                            }
                        }
                    case "gzip":
                        {
                            using (var contentStream = new MemoryStream())
                            {
                                using (var compressionStream = new GZipStream(contentStream, CompressionLevel.Optimal))
                                {
                                    compressionStream.Write(content, 0, content.Length);
                                }

                                return contentStream.ToArray();
                            }
                        }
                    case "br":
                        {
                            using (var contentStream = new MemoryStream())
                            {
                                using (var compressionStream = new BrotliStream(contentStream, CompressionLevel.Optimal))
                                {
                                    compressionStream.Write(content, 0, content.Length);
                                }

                                return contentStream.ToArray();
                            }
                        }
                    default:
                        {
                            using (var contentStream = new MemoryStream())
                            {
                                for (var i = 0; i < content.Length; i++)
                                {
                                    contentStream.WriteByte((byte)(content[i] ^ 0xFF));
                                }

                                return contentStream.ToArray();
                            }
                        }
                }
            }
        }
    }
}
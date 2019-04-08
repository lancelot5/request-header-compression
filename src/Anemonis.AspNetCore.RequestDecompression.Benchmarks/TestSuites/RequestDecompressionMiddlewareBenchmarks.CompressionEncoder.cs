using System.IO;
using System.IO.Compression;

namespace Anemonis.AspNetCore.RequestDecompression.Benchmarks.TestSuites
{
    public partial class RequestDecompressionMiddlewareBenchmarks
    {
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
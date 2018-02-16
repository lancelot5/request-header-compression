## Community.AspNetCore.RequestDecompression

Transparent HTTP request decompression middleware for ASP.NET Core 2.0, which serves as a complementary component to the [Microsoft.AspNetCore.ResponseCompression](https://www.nuget.org/packages/Microsoft.AspNetCore.ResponseCompression/).

[![NuGet package](https://img.shields.io/nuget/v/Community.AspNetCore.RequestDecompression.svg?style=flat-square)](https://www.nuget.org/packages/Community.AspNetCore.RequestDecompression)

### Features

- The middleware includes decompression providers for the `gzip` and `DEFLATE` algorithms.
- The middleware supports decompressing of content with multiple encodings.
- The middleware provides an ability to use a custom provider for the particular encoding.
- The middleware supports responding with HTTP status code `415` if an unsupported encoding is found.
- The middleware supports automatic disposing of providers which implement the `IDisposable` interface.
- The middleware supports the `identity` encoding by default.

### Examples

```cs
public class BrotliDecompressionProvider : IDecompressionProvider
{
    public Stream CreateStream(Stream outputStream)
    {
        return new BrotliStream(outputStream, CompressionMode.Decompress);
    }
}
```
\+
```cs
var options = new RequestDecompressionOptions();

options.Register<DeflateDecompressionProvider>("deflate");
options.Register<GzipDecompressionProvider>("gzip");
options.Register<BrotliDecompressionProvider>("br");
options.SkipUnsupportedEncodings = false;
```
\+
```cs
builder
    .ConfigureServices(sc => sc.AddRequestDecompression(options))
    .Configure(ab => ab.UseResponseCompression())
```
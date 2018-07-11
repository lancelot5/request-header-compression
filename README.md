## Community.AspNetCore.RequestDecompression

Transparent HTTP request decompression middleware for ASP.NET Core 2, which serves as a complementary component to the [Microsoft.AspNetCore.ResponseCompression](https://www.nuget.org/packages/Microsoft.AspNetCore.ResponseCompression/) package.

[![NuGet package](https://img.shields.io/nuget/v/Community.AspNetCore.RequestDecompression.svg?style=flat-square)](https://www.nuget.org/packages/Community.AspNetCore.RequestDecompression)

### Features

- The middleware includes decompression providers for the `gzip`, `DEFLATE`, and `Brotli` algorithms.
- The middleware supports decoding of content with multiple encodings.
- The middleware provides an ability to use a custom provider for the particular encoding.
- The middleware supports responding with HTTP status code `415` if an unsupported encoding is found.
- The middleware supports automatic disposing of disposable decoding providers.
- The middleware supports the `identity` encoding value.

### Specifics

- The middleware adds the `Content-Length` header if all encodings were handled.
- The decompression provider for `Brotli` algorithm is available only for .NET Core 2.1 and higher.

### Examples

```cs
public class LzmaDecompressionProvider : IDecompressionProvider
{
    public Stream CreateStream(Stream outputStream)
    {
        return new LzmaStream(outputStream, CompressionMode.Decompress);
    }

    public string EncodingName
    {
        get => "lzma";
    }
}
```
```cs
var options = new RequestDecompressionOptions();

options.UseDefaults();
options.Register<LzmaDecompressionProvider>();
options.SkipUnsupportedEncodings = false;
```
```cs
builder
    .ConfigureServices(sc => sc.AddRequestDecompression(options))
    .Configure(ab => ab.UseRequestDecompression())
```
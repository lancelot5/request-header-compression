## Community.AspNetCore.RequestDecompression

ASP.NET Core 2.0 middleware for transparent HTTP request decompression, which serves as a complementary component to the [Microsoft.AspNetCore.ResponseCompression](https://www.nuget.org/packages/Microsoft.AspNetCore.ResponseCompression/).

[![NuGet package](https://img.shields.io/nuget/v/Community.AspNetCore.RequestDecompression.svg?style=flat-square)](https://www.nuget.org/packages/Community.AspNetCore.RequestDecompression)

### Features

- The middleware includes decompression providers for `gzip` and `DEFLATE` algorithms.
- The middleware supports decompressing content with multiple encodings.
- The middleware provides an ability to use a custom provider for the particular encoding.
- The middleware supports `identity` encoding by default.

### Examples

```cs
var options = new RequestDecompressionOptions();

options.Register<DeflateDecompressionProvider>("deflate");
options.Register<GzipDecompressionProvider>("gzip");
```
\+
```cs
builder
    .ConfigureServices(sc => sc.AddRequestDecompression(options))
    .Configure(ab => ab.UseResponseCompression())
```
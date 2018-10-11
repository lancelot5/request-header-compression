# Anemonis.AspNetCore.RequestDecompression

Transparent HTTP request decompression middleware for ASP.NET Core 2, which serves as a complementary component to the [Microsoft.AspNetCore.ResponseCompression](https://www.nuget.org/packages/Microsoft.AspNetCore.ResponseCompression/) package.

[![NuGet package](https://img.shields.io/nuget/v/Anemonis.AspNetCore.RequestDecompression.svg?style=flat-square)](https://www.nuget.org/packages/Anemonis.AspNetCore.RequestDecompression)

## Project Details

- The middleware includes decompression providers for the `gzip`, `DEFLATE`, and `Brotli` algorithms.
- The middleware supports decoding of content with multiple encodings.
- The middleware provides an ability to use a custom provider for the particular encoding.
- The middleware supports responding with HTTP status code `415` if an unsupported encoding is found.
- The middleware supports automatic disposing of disposable decoding providers.
- The middleware supports the `identity` encoding value.
- The middleware adds the `Content-Length` header if all encodings were handled.

## Code Examples

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

public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var options = new RequestDecompressionOptions();

        options.UseDefaults();
        options.AddProvider<LzmaDecompressionProvider>();
        options.SkipUnsupportedEncodings = false;

        services.AddRequestDecompression(options);
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRequestDecompression();
    }
}
```

## Quicklinks

- [Contributing Guidelines](./CONTRIBUTING.md)
- [Code of Conduct](./CODE_OF_CONDUCT.md)
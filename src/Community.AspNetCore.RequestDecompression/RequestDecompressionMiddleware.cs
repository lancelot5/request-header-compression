using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Community.AspNetCore.RequestDecompression
{
    internal sealed class RequestDecompressionMiddleware : IMiddleware, IDisposable
    {
        private readonly IReadOnlyDictionary<string, IDecompressionProvider> _providers;
        private readonly bool _skipUnsupportedEncodings;

        public RequestDecompressionMiddleware(IServiceProvider services, IOptions<RequestDecompressionOptions> options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var providers = new Dictionary<string, IDecompressionProvider>(options.Value.Providers.Count, StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in options.Value.Providers)
            {
                providers[kvp.Key] = (IDecompressionProvider)ActivatorUtilities.CreateInstance(services, kvp.Value);
            }

            _providers = providers;
            _skipUnsupportedEncodings = options.Value.SkipUnsupportedEncodings;
        }

        async Task IMiddleware.InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var decodedStream = default(Stream);
            var encodingNames = (string[])context.Request.Headers[HeaderNames.ContentEncoding];

            if (encodingNames.Length > 0)
            {
                var encodingsLeft = encodingNames.Length;
                var decodingStream = context.Request.Body;

                for (var i = encodingNames.Length - 1; i >= 0; i--)
                {
                    if (string.Compare(encodingNames[i], "identity", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        encodingsLeft--;

                        continue;
                    }
                    if (!_providers.TryGetValue(encodingNames[i], out var provider))
                    {
                        if (!_skipUnsupportedEncodings)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;

                            return;
                        }

                        break;
                    }

                    decodingStream = provider.CreateStream(decodingStream);
                    encodingsLeft--;
                }

                if (decodingStream != context.Request.Body)
                {
                    decodedStream = new MemoryStream();

                    // 81920 is the default buffer size

                    await decodingStream.CopyToAsync(decodedStream, 81920, context.RequestAborted);

                    decodingStream.Dispose();
                    decodedStream.Position = 0L;
                    context.Request.Body = decodedStream;
                }

                if (encodingsLeft == 0)
                {
                    context.Request.Headers[HeaderNames.ContentLength] = context.Request.Body.Length.ToString(CultureInfo.InvariantCulture);
                    context.Request.Headers.Remove(HeaderNames.ContentEncoding);
                }
                else if (encodingsLeft < encodingNames.Length)
                {
                    var encodingNamesLeft = new string[encodingsLeft];

                    for (var i = 0; i < encodingNamesLeft.Length; i++)
                    {
                        encodingNamesLeft[i] = encodingNames[i];
                    }

                    context.Request.Headers[HeaderNames.ContentEncoding] = encodingNamesLeft;
                }
            }

            await next.Invoke(context);

            decodedStream?.Dispose();
        }

        void IDisposable.Dispose()
        {
            foreach (var provider in _providers.Values)
            {
                (provider as IDisposable)?.Dispose();
            }
        }
    }
}
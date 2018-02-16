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
                if (!_skipUnsupportedEncodings)
                {
                    for (var i = encodingNames.Length - 1; i >= 0; i--)
                    {
                        if (string.Compare(encodingNames[i], "identity", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            continue;
                        }
                        if (!_providers.ContainsKey(encodingNames[i]))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;

                            return;
                        }
                    }
                }

                var encodedStream = context.Request.Body;
                var encodingsLeft = encodingNames.Length;

                for (var i = encodingNames.Length - 1; i >= 0; i--)
                {
                    if (string.Compare(encodingNames[i], "identity", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        encodingsLeft--;

                        continue;
                    }
                    if (!_providers.TryGetValue(encodingNames[i], out var provider))
                    {
                        break;
                    }

                    decodedStream = new MemoryStream();

                    using (var decompressionStream = provider.CreateStream(encodedStream))
                    {
                        // 81920 is the default buffer size

                        await decompressionStream.CopyToAsync(decodedStream, 81920, context.RequestAborted);
                    }

                    decodedStream.Position = 0L;
                    encodedStream.Dispose();
                    encodedStream = decodedStream;
                    encodingsLeft--;
                }

                if (decodedStream != null)
                {
                    context.Request.Body = decodedStream;
                }

                if (encodingsLeft == 0)
                {
                    var contentStream = decodedStream ?? context.Request.Body;

                    context.Request.Headers[HeaderNames.ContentLength] = contentStream.Length.ToString(CultureInfo.InvariantCulture);
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
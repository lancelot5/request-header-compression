// © Alexander Kozlenko. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Anemonis.AspNetCore.RequestDecompression.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Anemonis.AspNetCore.RequestDecompression
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

            foreach (var type in options.Value.Providers)
            {
                var provider = (IDecompressionProvider)ActivatorUtilities.CreateInstance(services, type);
                var encodingName = provider.EncodingName;

                if (providers.ContainsKey(encodingName))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Strings.GetString("encoding.duplicate"), encodingName));
                }
                if (string.Equals(encodingName, "identity", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(Strings.GetString("encoding.identity"));
                }

                providers[encodingName] = provider;
            }

            providers["identity"] = null;

            _providers = providers;
            _skipUnsupportedEncodings = options.Value.SkipUnsupportedEncodings;
        }

        async Task IMiddleware.InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var decodedStream = default(Stream);

            if (context.Request.Headers.TryGetValue(HeaderNames.ContentEncoding, out var encodingNames) && (encodingNames.Count > 0))
            {
                var encodingsLeft = encodingNames.Count;
                var decodingStream = context.Request.Body;

                for (var i = encodingNames.Count - 1; i >= 0; i--)
                {
                    if (!_providers.TryGetValue(encodingNames[i], out var provider))
                    {
                        if (!_skipUnsupportedEncodings)
                        {
                            context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;

                            return;
                        }

                        break;
                    }
                    if (provider != null)
                    {
                        decodingStream = provider.CreateStream(decodingStream);
                    }

                    encodingsLeft--;
                }

                if (decodingStream != context.Request.Body)
                {
                    decodedStream = new MemoryStream();

                    using (decodingStream)
                    {
                        await decodingStream.CopyToAsync(decodedStream, 81920, context.RequestAborted);
                    }

                    decodedStream.Position = 0L;
                    context.Request.Body = decodedStream;
                }

                if (encodingsLeft == 0)
                {
                    context.Request.ContentLength = context.Request.Body.Length;
                    context.Request.Headers.Remove(HeaderNames.ContentEncoding);
                }
                else if (encodingsLeft < encodingNames.Count)
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
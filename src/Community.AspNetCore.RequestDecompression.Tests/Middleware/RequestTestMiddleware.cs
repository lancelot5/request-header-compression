using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Community.AspNetCore.RequestDecompression.Tests.Middleware
{
    /// <summary>Represents request testing middleware.</summary>
    internal sealed class RequestTestMiddleware : IMiddleware
    {
        private readonly Action<HttpRequest> _action;

        /// <summary>Initializes a new instance of the <see cref="RequestTestMiddleware" /> class.</summary>
        public RequestTestMiddleware(IOptions<RequestTestOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _action = options.Value.Action;
        }

        Task IMiddleware.InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _action?.Invoke(context.Request);

            return Task.CompletedTask;
        }
    }
}
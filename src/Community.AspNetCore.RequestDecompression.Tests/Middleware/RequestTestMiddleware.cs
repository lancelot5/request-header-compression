using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Community.AspNetCore.RequestDecompression.Tests.Middleware
{
    internal sealed class RequestTestMiddleware : IMiddleware
    {
        private readonly Func<HttpRequest, Task> _action;

        public RequestTestMiddleware(IOptions<RequestTestOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _action = options.Value.Action;

            if (options.Value.Action == null)
            {
                throw new InvalidOperationException("The action is not specified");
            }
        }

        Task IMiddleware.InvokeAsync(HttpContext context, RequestDelegate next)
        {
            return _action.Invoke(context.Request);
        }
    }
}
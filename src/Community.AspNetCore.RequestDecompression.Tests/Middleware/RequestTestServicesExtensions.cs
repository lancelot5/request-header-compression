using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Community.AspNetCore.RequestDecompression.Tests.Middleware
{
    internal static class RequestTestServicesExtensions
    {
        public static IServiceCollection AddRequestTest(this IServiceCollection services, Func<HttpRequest, Task> action)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            services.Configure<RequestTestOptions>(o => o.Action = action);
            services.AddSingleton<RequestTestMiddleware, RequestTestMiddleware>();

            return services;
        }
    }
}
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Anemonis.AspNetCore.RequestDecompression.IntegrationTests.TestStubs
{
    internal static class RequestTestServicesExtensions
    {
        public static IServiceCollection AddRequestTest(this IServiceCollection services, Action<HttpRequest> action)
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
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Community.AspNetCore.RequestDecompression.Tests.Middleware
{
    /// <summary>The request testing middleware extensions for the <see cref="IServiceCollection" />.</summary>
    internal static class RequestTestServicesExtensions
    {
        /// <summary>Adds the request testing middleware services to the current <see cref="IServiceCollection" /> instance.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> instance to add the services to.</param>
        /// <param name="action">The request testing action to use.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="services" /> or <paramref name="action" /> is <see langword="null" />.</exception>
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
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Community.AspNetCore.RequestDecompression
{
    /// <summary>The HTTP request decompression middleware extensions for the <see cref="IServiceCollection" />.</summary>
    public static class RequestDecompressionServicesExtensions
    {
        /// <summary>Adds the HTTP request decompression middleware services to the current <see cref="IServiceCollection" /> instance.</summary>
        /// <param name="services">The <see cref="IServiceCollection" /> instance to add the services to.</param>
        /// <param name="options">The middleware options to add to the current <see cref="IServiceCollection" /> instance.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="services" /> is <see langword="null" />.</exception>
        public static IServiceCollection AddRequestDecompression(this IServiceCollection services, RequestDecompressionOptions options = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options != null)
            {
                services.Configure<RequestDecompressionOptions>(o => o.Apply(options));
            }

            services.AddSingleton<RequestDecompressionMiddleware, RequestDecompressionMiddleware>();

            return services;
        }
    }
}
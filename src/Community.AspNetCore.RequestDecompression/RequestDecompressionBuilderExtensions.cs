using System;
using Microsoft.AspNetCore.Builder;

namespace Community.AspNetCore.RequestDecompression
{
    /// <summary>The HTTP request decompression middleware extensions for the <see cref="IApplicationBuilder" />.</summary>
    public static class RequestDecompressionBuilderExtensions
    {
        /// <summary>Adds the HTTP request decompression middleware to the application's request pipeline.</summary>
        /// <param name="builder">The <see cref="IApplicationBuilder" /> to add the middleware to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="builder" /> is <see langword="null" />.</exception>
        public static IApplicationBuilder UseRequestDecompression(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<RequestDecompressionMiddleware>();
        }
    }
}
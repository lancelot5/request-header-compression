using System;
using Microsoft.AspNetCore.Builder;

namespace Anemonis.AspNetCore.RequestDecompression.IntegrationTests.Middleware
{
    internal static class RequestTestBuilderExtensions
    {
        public static IApplicationBuilder UseRequestTest(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<RequestTestMiddleware>();
        }
    }
}
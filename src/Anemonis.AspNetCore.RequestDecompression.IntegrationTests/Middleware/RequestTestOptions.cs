using System;
using Microsoft.AspNetCore.Http;

namespace Anemonis.AspNetCore.RequestDecompression.IntegrationTests.Middleware
{
    internal sealed class RequestTestOptions
    {
        public RequestTestOptions()
        {
        }

        public Action<HttpRequest> Action
        {
            get;
            set;
        }
    }
}
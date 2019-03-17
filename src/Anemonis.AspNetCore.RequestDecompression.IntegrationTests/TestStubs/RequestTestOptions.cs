using System;
using Microsoft.AspNetCore.Http;

namespace Anemonis.AspNetCore.RequestDecompression.IntegrationTests.TestStubs
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
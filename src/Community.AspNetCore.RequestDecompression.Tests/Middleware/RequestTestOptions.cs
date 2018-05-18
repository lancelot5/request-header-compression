using System;
using Microsoft.AspNetCore.Http;

namespace Community.AspNetCore.RequestDecompression.Tests.Middleware
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
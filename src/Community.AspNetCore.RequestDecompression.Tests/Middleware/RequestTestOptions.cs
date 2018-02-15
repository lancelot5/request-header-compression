using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Community.AspNetCore.RequestDecompression.Tests.Middleware
{
    internal sealed class RequestTestOptions
    {
        public Func<HttpRequest, Task> Action
        {
            get;
            set;
        }
    }
}
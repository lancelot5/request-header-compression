using System;
using Microsoft.AspNetCore.Http;

namespace Community.AspNetCore.RequestDecompression.Tests.Middleware
{
    /// <summary>Provides options for the request testing middleware.</summary>
    internal sealed class RequestTestOptions
    {
        /// <summary>Initializes a new instance of the <see cref="RequestTestOptions" /> class.</summary>
        public RequestTestOptions()
        {
        }

        /// <summary>Gets or sets the request testing action.</summary>
        public Action<HttpRequest> Action
        {
            get;
            set;
        }
    }
}
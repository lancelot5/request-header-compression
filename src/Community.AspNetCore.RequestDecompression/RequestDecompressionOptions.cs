using System;
using System.Collections.Generic;

namespace Community.AspNetCore.RequestDecompression
{
    /// <summary>Provides options for the HTTP request decompression middleware.</summary>
    public sealed class RequestDecompressionOptions
    {
        private readonly HashSet<Type> _providers = new HashSet<Type>();

        /// <summary>Initializes a new instance of the <see cref="RequestDecompressionOptions" /> class.</summary>
        public RequestDecompressionOptions()
        {
            SkipUnsupportedEncodings = true;
        }

        internal void Apply(RequestDecompressionOptions options)
        {
            foreach (var type in options.Providers)
            {
                _providers.Add(type);
            }

            SkipUnsupportedEncodings = options.SkipUnsupportedEncodings;
        }

        /// <summary>Registers the decompression provider.</summary>
        /// <typeparam name="T">The type of the decompression provider.</typeparam>
        public void Register<T>()
            where T : class, IDecompressionProvider
        {
            _providers.Add(typeof(T));
        }

        /// <summary>Gets the collection of decompression provider types.</summary>
        public IReadOnlyCollection<Type> Providers
        {
            get => _providers;
        }

        /// <summary>Gets or sets the value indicating whether the middleware should pass content with unsupported encoding to the next middleware in the request pipeline.</summary>
        public bool SkipUnsupportedEncodings
        {
            get;
            set;
        }
    }
}
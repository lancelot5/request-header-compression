// © Alexander Kozlenko. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Anemonis.AspNetCore.RequestDecompression.Resources;

namespace Anemonis.AspNetCore.RequestDecompression
{
    /// <summary>Provides options for the HTTP request decompression middleware.</summary>
    public sealed class RequestDecompressionOptions
    {
        private readonly HashSet<Type> _providers = new HashSet<Type>();

        private bool _skipUnsupportedEncodings = true;

        /// <summary>Initializes a new instance of the <see cref="RequestDecompressionOptions" /> class.</summary>
        public RequestDecompressionOptions()
        {
        }

        /// <summary>Adds the specified decompression provider type.</summary>
        /// <param name="type">The type of the decompression provider.</param>
        /// <exception cref="ArgumentException"><paramref name="type" /> does not implement the <see cref="IDecompressionProvider" /> interface.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <see langword="null" />.</exception>
        public void AddProvider(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (!typeof(IDecompressionProvider).IsAssignableFrom(type))
            {
                throw new ArgumentException(Strings.GetString("provider.invalid_type"), nameof(type));
            }

            _providers.Add(type);
        }

        /// <summary>Adds the specified decompression provider type.</summary>
        /// <typeparam name="T">The type of the decompression provider.</typeparam>
        public void AddProvider<T>()
            where T : IDecompressionProvider
        {
            _providers.Add(typeof(T));
        }

        /// <summary>Gets a collection of registered decompression provider types.</summary>
        public IReadOnlyCollection<Type> Providers
        {
            get => _providers;
        }

        /// <summary>Gets or sets the value indicating whether the middleware should pass content with unsupported encoding to the next middleware in the request pipeline.</summary>
        public bool SkipUnsupportedEncodings
        {
            get => _skipUnsupportedEncodings;
            set => _skipUnsupportedEncodings = value;
        }
    }
}
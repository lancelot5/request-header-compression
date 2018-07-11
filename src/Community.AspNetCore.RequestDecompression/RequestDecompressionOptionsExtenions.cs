using System;

namespace Community.AspNetCore.RequestDecompression
{
    /// <summary>The extensions for the <see cref="RequestDecompressionOptions" /> class.</summary>
    public static class RequestDecompressionOptionsExtenions
    {
        /// <summary>Setups default values and registers default compression providers.</summary>
        /// <param name="options">The middleware options to modify.</param>
        /// <exception cref="ArgumentNullException"><paramref name="options" /> is <see langword="null" />.</exception>
        public static void UseDefaults(this RequestDecompressionOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Register<DeflateDecompressionProvider>();
            options.Register<GzipDecompressionProvider>();
#if NETCOREAPP2_1
            options.Register<BrotliDecompressionProvider>();
#endif
            options.SkipUnsupportedEncodings = true;
        }
    }
}
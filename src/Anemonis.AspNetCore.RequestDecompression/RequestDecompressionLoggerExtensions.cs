// © Alexander Kozlenko. Licensed under the MIT License.

using System;
using Anemonis.AspNetCore.RequestDecompression.Resources;
using Microsoft.Extensions.Logging;

namespace Anemonis.AspNetCore.RequestDecompression
{
    internal static class RequestDecompressionLoggerExtensions
    {
        private static readonly Action<ILogger, Exception> _logRequestContentIsNotEncoded;
        private static readonly Action<ILogger, int, Exception> _logRequestContentIsEncoded;
        private static readonly Action<ILogger, Type, Exception> _logRequestDecodingApplied;
        private static readonly Action<ILogger, Exception> _logRequestDecodingSkipped;
        private static readonly Action<ILogger, Exception> _logRequestDecodingDisabled;

        static RequestDecompressionLoggerExtensions()
        {
            _logRequestContentIsNotEncoded = LoggerMessage.Define(
                LogLevel.Trace,
                new EventId(1000, "REQDEC_CONTENT_NOT_ENCODED"),
                Strings.GetString("logging.content_not_encoded"));
            _logRequestContentIsEncoded = LoggerMessage.Define<int>(
                LogLevel.Trace,
                new EventId(1001, "REQDEC_CONTENT_ENCODED"),
                Strings.GetString("logging.content_encoded"));
            _logRequestDecodingApplied = LoggerMessage.Define<Type>(
                LogLevel.Debug,
                new EventId(1100, "REQDEC_DECODING_APPLIED"),
                Strings.GetString("logging.decoding_applied"));
            _logRequestDecodingSkipped = LoggerMessage.Define(
                LogLevel.Debug,
                new EventId(1101, "REQDEC_DECODING_SKIPPED"),
                Strings.GetString("logging.decoding_skipped"));
            _logRequestDecodingDisabled = LoggerMessage.Define(
                LogLevel.Warning,
                new EventId(1300, "REQDEC_DECODING_DISABLED"),
                Strings.GetString("logging.decoding_disabled"));
        }

        public static void LogRequestContentIsNotEncoded(this ILogger logger)
        {
            _logRequestContentIsNotEncoded.Invoke(logger, null);
        }

        public static void LogRequestContentIsEncoded(this ILogger logger, int count)
        {
            _logRequestContentIsEncoded.Invoke(logger, count, null);
        }

        public static void LogRequestDecodingApplied(this ILogger logger, Type type)
        {
            _logRequestDecodingApplied.Invoke(logger, type, null);
        }

        public static void LogRequestDecodingSkipped(this ILogger logger)
        {
            _logRequestDecodingSkipped.Invoke(logger, null);
        }

        public static void LogRequestDecodingDisabled(this ILogger logger)
        {
            _logRequestDecodingDisabled.Invoke(logger, null);
        }
    }
}
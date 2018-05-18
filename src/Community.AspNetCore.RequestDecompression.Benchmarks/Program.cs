using System.Linq;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Community.AspNetCore.RequestDecompression.Benchmarks.Framework;
using Community.AspNetCore.RequestDecompression.Benchmarks.Suites;

namespace Community.AspNetCore.RequestDecompression.Benchmarks
{
    public static class Program
    {
        public static void Main()
        {
            var configuration = ManualConfig.CreateEmpty();

            configuration.Add(new SimpleBenchmarkExporter());
            configuration.Add(MemoryDiagnoser.Default);
            configuration.Add(ConsoleLogger.Default);
            configuration.Add(DefaultConfig.Instance.GetColumnProviders().ToArray());
            configuration.Set(SummaryStyle.Default.WithSizeUnit(SizeUnit.B));

            BenchmarkRunner.Run(typeof(RequestDecompressionMiddlewareBenchmarks), configuration);
        }
    }
}
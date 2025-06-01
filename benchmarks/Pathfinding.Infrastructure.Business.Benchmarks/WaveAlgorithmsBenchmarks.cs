using BenchmarkDotNet.Attributes;
using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Infrastructure.Business.Benchmarks.Data;

namespace Pathfinding.Infrastructure.Business.Benchmarks;

[MemoryDiagnoser]
public class WaveAlgorithmsBenchmarks
{
    private static List<BenchmarkVertex> range;

    [GlobalSetup]
    public static void Setup()
    {
        range = [.. BenchmarkRange.Interface];
    }

    [Benchmark(Baseline = true)]
    public static void DijkstraAlgorithmBenchmark()
    {
        var algorithm = new DijkstraAlgorithm(range);

        algorithm.FindPath();
    }

    [Benchmark]
    public static void BidirectDijkstraAlgorithmBenchmark()
    {
        var algorithm = new BidirectDijkstraAlgorithm(range);

        algorithm.FindPath();
    }

    [Benchmark]
    public static void BidirectAStarDijkstraAlgorithmBenchmark()
    {
        var algorithm = new BidirectAStarAlgorithm(range);

        algorithm.FindPath();
    }


    [Benchmark]
    public static void AStarAlgorithmBenchmark()
    {
        var algorithm = new AStarAlgorithm(range);

        algorithm.FindPath();
    }
}
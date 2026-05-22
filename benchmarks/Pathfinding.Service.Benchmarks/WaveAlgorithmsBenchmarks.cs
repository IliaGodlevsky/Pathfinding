using BenchmarkDotNet.Attributes;
using Pathfinding.Service.Algorithms;
using Pathfinding.Service.Benchmarks.Data;

namespace Pathfinding.Service.Benchmarks;

[MemoryDiagnoser]
public class WaveAlgorithmsBenchmarks
{
    private List<BenchmarkVertex> range;

    [GlobalSetup]
    public void Setup()
    {
        range = [.. BenchmarkRange.Interface];
    }

    [Benchmark(Baseline = true)]
    public void DijkstraAlgorithmBenchmark()
    {
        var algorithm = new DijkstraAlgorithm(range);

        algorithm.FindPath();
    }

    [Benchmark]
    public void BidirectDijkstraAlgorithmBenchmark()
    {
        var algorithm = new BidirectDijkstraAlgorithm(range);

        algorithm.FindPath();
    }

    [Benchmark]
    public void BidirectAStarDijkstraAlgorithmBenchmark()
    {
        var algorithm = new BidirectAStarAlgorithm(range);

        algorithm.FindPath();
    }


    [Benchmark]
    public void AStarAlgorithmBenchmark()
    {
        var algorithm = new AStarAlgorithm(range);

        algorithm.FindPath();
    }
}
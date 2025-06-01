using BenchmarkDotNet.Attributes;
using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Infrastructure.Business.Benchmarks.Data;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Benchmarks;

public class HeuristicsBenchmarks
{
    private static BenchmarkVertex first;
    private static BenchmarkVertex second;

    [GlobalSetup]
    public static void Setup()
    {
        first = new BenchmarkVertex() { Position = new Coordinate(2, 4) };
        second = new BenchmarkVertex() { Position = new Coordinate(7, 11) };
    }

    [Benchmark]
    public static void ChebyshevDistanceBenchmark()
    {
        var chebyshev = new ChebyshevDistance();

        chebyshev.Calculate(first, second);
    }

    [Benchmark(Baseline = true)]
    public static void ManhattanDistanceBenchmark()
    {
        var chebyshev = new ManhattanDistance();

        chebyshev.Calculate(first, second);
    }

    [Benchmark]
    public static void EuclidianDistanceBenchmark()
    {
        var chebyshev = new EuclideanDistance();

        chebyshev.Calculate(first, second);
    }
}
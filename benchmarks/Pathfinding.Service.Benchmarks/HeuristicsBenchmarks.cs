using BenchmarkDotNet.Attributes;
using Pathfinding.Service.Algorithms.Heuristics;
using Pathfinding.Service.Benchmarks.Data;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Service.Benchmarks;

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
    public void ChebyshevDistanceBenchmark()
    {
        var chebyshev = new ChebyshevDistance();

        chebyshev.Calculate(first, second);
    }

    [Benchmark(Baseline = true)]
    public void ManhattanDistanceBenchmark()
    {
        var chebyshev = new ManhattanDistance();

        chebyshev.Calculate(first, second);
    }

    [Benchmark]
    public void EuclidianDistanceBenchmark()
    {
        var chebyshev = new EuclideanDistance();

        chebyshev.Calculate(first, second);
    }

    [Benchmark]
    public void DiagonalDistanceBenchmark()
    {
        var chebyshev = new DiagonalDistance();

        chebyshev.Calculate(first, second);
    }
}
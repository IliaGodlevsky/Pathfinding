using BenchmarkDotNet.Attributes;
using Pathfinding.Data;
using Pathfinding.Service.Algorithms.StepRules;
using Pathfinding.Service.Benchmarks.Data;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Service.Benchmarks;

public class StepRulesBenchmarks
{
    private static BenchmarkVertex first;
    private static BenchmarkVertex second;

    [GlobalSetup]
    public static void Setup()
    {
        first = new BenchmarkVertex()
        {
            Position = new Coordinate(2, 3),
            Cost = new VertexCost(3)
        };
        second = new BenchmarkVertex()
        {
            Position = new Coordinate(12, 45),
            Cost = new VertexCost(6)
        };
    }

    [Benchmark(Baseline = true)]
    public static void DefaultStepRuleBenchmark()
    {
        var stepRule = new DefaultStepRule();

        stepRule.CalculateStepCost(first, second);
    }

    [Benchmark]
    public static void LandscapeStepRuleBenchmark()
    {
        var stepRule = new LandscapeStepRule();

        stepRule.CalculateStepCost(first, second);
    }
}
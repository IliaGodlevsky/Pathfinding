using BenchmarkDotNet.Attributes;
using Pathfinding.Infrastructure.Business.Algorithms.StepRules;
using Pathfinding.Infrastructure.Business.Benchmarks.Data;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Benchmarks
{
    public class StepRulesBenchmarks
    {
        private static BenchmarkVertex first;
        private static BenchmarkVertex second;

        [GlobalSetup]
        public void Setup()
        {
            var range = new InclusiveValueRange<int>(9, 1);
            first = new BenchmarkVertex()
            {
                Position = new Coordinate(2, 3),
                Cost = new VertexCost(3, range)
            };
            second = new BenchmarkVertex()
            {
                Position = new Coordinate(12, 45),
                Cost = new VertexCost(6, range)
            };
        }

        [Benchmark(Baseline = true)]
        public void DefaultStepRuleBenchmark()
        {
            var stepRule = new DefaultStepRule();

            stepRule.CalculateStepCost(first, second);
        }

        [Benchmark]
        public void LandscapeStepRuleBenchmark()
        {
            var stepRule = new LandscapeStepRule();

            stepRule.CalculateStepCost(first, second);
        }
    }
}

using Autofac.Features.Metadata;
using Pathfinding.App.Console.Injection;
using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Service.Interface;

namespace Pathfinding.App.Console.Factories;

public sealed class HeuristicsFactory(IEnumerable<Meta<IHeuristic>> heuristics) 
    : IHeuristicsFactory
{
    private readonly Dictionary<Heuristic, IHeuristic> heuristics 
        = heuristics.ToDictionary(
            x => (Heuristic)x.Metadata[MetadataKeys.Heuristics],
            x => x.Value);

    public IReadOnlyCollection<Heuristic> Allowed => heuristics.Keys;

    public IHeuristic CreateHeuristic(Heuristic heuristic, double weight)
    {
        if (heuristics.TryGetValue(heuristic, out var value))
        {
            return new WeightedHeuristic(value, weight);
        }

        throw new KeyNotFoundException($"{heuristic} was not found");
    }
}
using Autofac.Features.Metadata;
using Pathfinding.App.Console.Injection;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Service.Interface;

namespace Pathfinding.App.Console.Factories;

public sealed class HeuristicsFactory(IEnumerable<Meta<IHeuristic>> heuristics) 
    : IHeuristicsFactory
{
    private readonly Dictionary<Heuristics, IHeuristic> heuristics 
        = heuristics.ToDictionary(
            x => (Heuristics)x.Metadata[MetadataKeys.Heuristics],
            x => x.Value);

    public IReadOnlyCollection<Heuristics> Allowed => heuristics.Keys;

    public IHeuristic CreateHeuristic(Heuristics heuristic, double weight)
    {
        if (heuristics.TryGetValue(heuristic, out var value))
        {
            return new WeightedHeuristic(value, weight);
        }

        throw new KeyNotFoundException($"{heuristic} was not found");
    }
}
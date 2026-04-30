using Autofac.Features.Metadata;
using Pathfinding.App.Console.Injection;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Service.Interface;

namespace Pathfinding.App.Console.Factories;

public sealed class HeuristicsFactory(IEnumerable<Meta<IHeuristic>> heuristics)
    : IHeuristicsFactory
{
    private readonly Dictionary<Heuristics, IHeuristic> heuristics
        = heuristics.ToDictionary(
            x => (Heuristics)x.Metadata[MetadataKeys.Heuristics],
            x => x.Value);

    public IReadOnlyCollection<Heuristics> AvailableHeuristics => heuristics.Keys;

    public IHeuristic CreateHeuristic(Heuristics heuristic, double weight)
    {
        var value = heuristics.GetValueOrDefault(heuristic)
            ?? throw new KeyNotFoundException($"{heuristic} was not found");
        return weight == 1 ? value : new WeightedHeuristic(value, weight);
    }
}
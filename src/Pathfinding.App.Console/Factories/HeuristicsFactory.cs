using Autofac.Features.Metadata;
using Pathfinding.App.Console.Injection;
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Service.Interface;
using Heuristic = Pathfinding.Domain.Core.Enums.Heuristics;

namespace Pathfinding.App.Console.Factories;

public sealed class HeuristicsFactory : IHeuristicsFactory
{
    private readonly Dictionary<Heuristic, IHeuristic> heuristics;

    public IReadOnlyCollection<Heuristic> Allowed => heuristics.Keys;

    public HeuristicsFactory(IEnumerable<Meta<IHeuristic>> heuristics)
    {
        this.heuristics = heuristics.ToDictionary(x => (Heuristic)x.Metadata[MetadataKeys.Heuristics], x => x.Value);
    }

    public IHeuristic CreateHeuristic(Heuristic heuristic, double weight)
    {
        if (heuristics.TryGetValue(heuristic, out var value))
        {
            return value.WithWeight(weight);
        }

        throw new KeyNotFoundException($"{heuristic} was not found");
    }
}
using Autofac.Features.Metadata;
using Pathfinding.App.Console.Injection;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Algorithms;

namespace Pathfinding.App.Console.Factories;

internal sealed class AlgorithmsFactory : IAlgorithmsFactory
{
    private readonly Dictionary<Algorithms, IAlgorithmFactory<PathfindingProcess>> algorithms;

    public IReadOnlyCollection<Algorithms> Allowed { get; }

    public AlgorithmsFactory(Meta<IAlgorithmFactory<PathfindingProcess>>[] algorithms)
    {
        this.algorithms = algorithms.ToDictionary(
            x => (Algorithms)x.Metadata[MetadataKeys.Algorithm], 
            x => x.Value);
        Allowed = [.. algorithms.OrderBy(x => x.Metadata[MetadataKeys.Order])
            .Select(x => (Algorithms)x.Metadata[MetadataKeys.Algorithm])];
    }

    public IAlgorithmFactory<PathfindingProcess> GetAlgorithmFactory(Algorithms algorithm)
    {
        if (algorithms.TryGetValue(algorithm, out var value))
        {
            return value;
        }

        throw new KeyNotFoundException($"{algorithm} was not found");
    }
}
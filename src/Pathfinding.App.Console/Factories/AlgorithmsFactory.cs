using Autofac.Features.Metadata;
using Pathfinding.App.Console.Injection;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Algorithms;

namespace Pathfinding.App.Console.Factories;

internal sealed class AlgorithmsFactory(Meta<IAlgorithmFactory<PathfindingProcess>>[] algorithms) : IAlgorithmsFactory
{
    private readonly Dictionary<Algorithms, IAlgorithmFactory<PathfindingProcess>> algorithms 
        = algorithms.ToDictionary(
            x => (Algorithms)x.Metadata[MetadataKeys.Algorithm],
            x => x.Value);

    public IReadOnlyCollection<Algorithms> Allowed { get; } 
        = [.. algorithms.OrderBy(x => x.Metadata[MetadataKeys.Order])
            .Select(x => (Algorithms)x.Metadata[MetadataKeys.Algorithm])];

    public IAlgorithmFactory<PathfindingProcess> GetAlgorithmFactory(Algorithms algorithm)
    {
        if (algorithms.TryGetValue(algorithm, out var value))
        {
            return value;
        }

        throw new KeyNotFoundException($"{algorithm} was not found");
    }
}
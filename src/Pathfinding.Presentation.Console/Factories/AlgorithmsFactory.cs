using Autofac.Features.Metadata;
using Pathfinding.Domain.Enums;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Models;
using Pathfinding.Service.Algorithms;

namespace Pathfinding.Presentation.Console.Factories;

internal sealed class AlgorithmsFactory(Meta<IAlgorithmFactory<PathfindingProcess>>[] algorithms) : IAlgorithmsFactory
{
    private readonly Dictionary<Algorithms, IAlgorithmFactory<PathfindingProcess>> algorithms
        = algorithms.ToDictionary(
            x => (Algorithms)x.Metadata[MetadataKeys.Algorithm],
            x => x.Value);

    public IReadOnlyList<Algorithms> AvailableAlgorithms { get; }
        = [.. algorithms.OrderBy(x => x.Metadata[MetadataKeys.Order])
            .Select(x => (Algorithms)x.Metadata[MetadataKeys.Algorithm])];

    public IReadOnlyDictionary<Algorithms, AlgorithmRequirements> Requirements { get; }
        = algorithms.ToDictionary(
            x => (Algorithms)x.Metadata[MetadataKeys.Algorithm],
            x => (AlgorithmRequirements)x.Metadata[MetadataKeys.Requirements]);

    public IAlgorithmFactory<PathfindingProcess> Create(Algorithms algorithm)
    {
        return algorithms.GetValueOrDefault(algorithm)
            ?? throw new KeyNotFoundException($"{algorithm} was not found");
    }
}
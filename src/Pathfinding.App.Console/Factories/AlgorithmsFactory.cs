using Autofac.Features.Metadata;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Models;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Algorithms;
using System.Collections.Generic;

namespace Pathfinding.App.Console.Factories;

internal sealed class AlgorithmsFactory(Meta<IAlgorithmFactory<PathfindingProcess>>[] algorithms) : IAlgorithmsFactory
{
    private readonly Dictionary<Algorithms, IAlgorithmFactory<PathfindingProcess>> algorithms
        = algorithms.ToDictionary(
            x => (Algorithms)x.Metadata[MetadataKeys.Algorithm],
            x => x.Value);

    public IReadOnlyList<Algorithms> Allowed { get; }
        = [.. algorithms.OrderBy(x => x.Metadata[MetadataKeys.Order])
            .Select(x => (Algorithms)x.Metadata[MetadataKeys.Algorithm])];

    public IReadOnlyDictionary<Algorithms, AlgorithmRequirements> Requirements { get; }
        = algorithms.ToDictionary(
            x => (Algorithms)x.Metadata[MetadataKeys.Algorithm], 
            x => (AlgorithmRequirements)x.Metadata[MetadataKeys.Requirements]);

    public IAlgorithmFactory<PathfindingProcess> GetAlgorithmFactory(Algorithms algorithm)
    {
        return algorithms.TryGetValue(algorithm, out var value)
            ? value
            : throw new KeyNotFoundException($"{algorithm} was not found");
    }
}
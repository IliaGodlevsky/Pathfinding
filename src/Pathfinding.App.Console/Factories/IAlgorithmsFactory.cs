using Pathfinding.App.Console.Models;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Algorithms;

namespace Pathfinding.App.Console.Factories;

public interface IAlgorithmsFactory
{
    IReadOnlyList<Algorithms> Allowed { get; }

    IReadOnlyDictionary<Algorithms, AlgorithmRequirements> Requirements { get; }

    IAlgorithmFactory<PathfindingProcess> GetAlgorithmFactory(Algorithms algorithm);
}
using Pathfinding.Domain.Enums;
using Pathfinding.Presentation.Console.Models;
using Pathfinding.Service.Algorithms;

namespace Pathfinding.Presentation.Console.Factories;

public interface IAlgorithmsFactory
{
    IReadOnlyList<Algorithms> AvailableAlgorithms { get; }

    IReadOnlyDictionary<Algorithms, AlgorithmRequirements> Requirements { get; }

    IAlgorithmFactory<PathfindingProcess> Create(Algorithms algorithm);
}
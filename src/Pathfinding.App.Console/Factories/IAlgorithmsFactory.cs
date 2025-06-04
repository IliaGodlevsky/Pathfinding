using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Algorithms;

namespace Pathfinding.App.Console.Factories;

public interface IAlgorithmsFactory
{
    IReadOnlyCollection<Algorithms> Allowed { get; }

    IAlgorithmFactory<PathfindingProcess> GetAlgorithmFactory(Algorithms algorithm);
}
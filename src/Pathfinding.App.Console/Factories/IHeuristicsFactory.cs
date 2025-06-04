using Pathfinding.Domain.Core.Enums;
using Pathfinding.Service.Interface;

namespace Pathfinding.App.Console.Factories;

public interface IHeuristicsFactory
{
    IReadOnlyCollection<Heuristics> Allowed { get; }

    IHeuristic CreateHeuristic(Heuristics heuristics, double weight);
}
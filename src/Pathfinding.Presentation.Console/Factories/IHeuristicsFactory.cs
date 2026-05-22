using Pathfinding.Domain.Enums;
using Pathfinding.Service.Interface;

namespace Pathfinding.Presentation.Console.Factories;

public interface IHeuristicsFactory
{
    IReadOnlyCollection<Heuristics> AvailableHeuristics { get; }

    IHeuristic Create(Heuristics heuristics, double weight);
}
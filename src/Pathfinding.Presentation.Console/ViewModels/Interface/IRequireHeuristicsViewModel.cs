using Pathfinding.Domain.Enums;

namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface IRequireHeuristicsViewModel
{
    IReadOnlyCollection<Heuristics> AvailableHeuristics { get; }

    IList<Heuristics> AppliedHeuristics { get; }
}
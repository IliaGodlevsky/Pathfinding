using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.ViewModels.Interface;

internal interface IRequireHeuristicsViewModel
{
    IReadOnlyCollection<Heuristics> AvailableHeuristics { get; }

    IList<Heuristics> AppliedHeuristics { get; }
}
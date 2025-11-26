using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.ViewModels.Interface;

internal interface IRequireHeuristicsViewModel
{
    IReadOnlyCollection<Heuristics> AllowedHeuristics { get; }

    IList<Heuristics> AppliedHeuristics { get; }
}
using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.ViewModel.Interface
{
    internal interface IRequireHeuristicsViewModel
    {
        HeuristicFunctions? Heuristic { get; set; }

        double? FromWeight { get; set; }

        double? ToWeight { get; set; }

        double? Step { get; set; }
    }
}

using Pathfinding.Domain.Core.Enums;
using System.ComponentModel;

namespace Pathfinding.App.Console.ViewModels.Interface
{
    internal interface IRequireHeuristicsViewModel
    {
        event PropertyChangedEventHandler PropertyChanged;

        HeuristicFunctions? Heuristic { get; set; }

        double? FromWeight { get; set; }

        double? ToWeight { get; set; }

        double? Step { get; set; }
    }
}

using Pathfinding.Domain.Core.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Pathfinding.App.Console.ViewModels.Interface
{
    internal interface IRequireHeuristicsViewModel
    {
        event PropertyChangedEventHandler PropertyChanged;

        ObservableCollection<Heuristics?> Heuristics { get; }

        double? FromWeight { get; set; }

        double? ToWeight { get; set; }

        double? Step { get; set; }
    }
}

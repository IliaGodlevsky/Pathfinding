using Pathfinding.Domain.Core.Enums;
using System.Collections.ObjectModel;

namespace Pathfinding.App.Console.ViewModels.Interface
{
    internal interface IRequireHeuristicsViewModel
    {
        IReadOnlyCollection<Heuristics> AllowedHeuristics { get; }

        ObservableCollection<Heuristics?> Heuristics { get; }
    }
}

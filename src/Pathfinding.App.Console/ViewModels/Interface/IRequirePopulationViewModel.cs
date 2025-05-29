using System.ComponentModel;

namespace Pathfinding.App.Console.ViewModels.Interface;

internal interface IRequirePopulationViewModel
{
    event PropertyChangedEventHandler PropertyChanged;

    double? FromWeight { get; set; }

    double? ToWeight { get; set; }

    double? Step { get; set; }
}
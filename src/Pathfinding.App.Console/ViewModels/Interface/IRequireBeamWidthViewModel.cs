using System.ComponentModel;

namespace Pathfinding.App.Console.ViewModels.Interface;

internal interface IRequireBeamWidthViewModel : INotifyPropertyChanged
{
    int? BeamWidth { get; set; }
}

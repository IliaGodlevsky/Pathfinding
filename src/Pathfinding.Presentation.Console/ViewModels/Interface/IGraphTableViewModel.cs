using Pathfinding.Presentation.Console.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;

namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface IGraphTableViewModel
{
    ObservableCollection<GraphInfoModel> Graphs { get; }

    ReactiveCommand<int, Unit> ActivateGraphCommand { get; }

    ReactiveCommand<int[], Unit> SelectGraphsCommand { get; }

    ReactiveCommand<Unit, Unit> LoadGraphsCommand { get; }
}
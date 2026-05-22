using Pathfinding.Presentation.Console.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;

namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface IRunsTableViewModel
{
    ObservableCollection<RunInfoModel> Runs { get; }

    ReactiveCommand<int[], Unit> SelectRunsCommand { get; }
}
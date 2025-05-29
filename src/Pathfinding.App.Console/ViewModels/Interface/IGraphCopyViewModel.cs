using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels.Interface;

internal interface IGraphCopyViewModel
{
    ReactiveCommand<Unit, Unit> CopyGraphCommand { get; }
}
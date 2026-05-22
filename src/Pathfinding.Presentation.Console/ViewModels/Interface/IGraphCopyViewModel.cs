using ReactiveUI;
using System.Reactive;

namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface IGraphCopyViewModel
{
    ReactiveCommand<Unit, Unit> CopyGraphCommand { get; }
}
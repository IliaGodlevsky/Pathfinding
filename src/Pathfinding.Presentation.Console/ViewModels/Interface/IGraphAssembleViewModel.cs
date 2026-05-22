using ReactiveUI;
using System.Reactive;

namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface IGraphAssembleViewModel
{
    ReactiveCommand<Unit, Unit> AssembleGraphCommand { get; }
}
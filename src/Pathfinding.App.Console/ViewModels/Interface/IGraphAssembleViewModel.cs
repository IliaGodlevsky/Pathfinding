using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels.Interface;

internal interface IGraphAssembleViewModel
{
    ReactiveCommand<Unit, Unit> AssembleGraphCommand { get; }
}
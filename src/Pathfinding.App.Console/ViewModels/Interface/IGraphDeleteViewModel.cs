using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels.Interface;

internal interface IGraphDeleteViewModel
{
    ReactiveCommand<Unit, Unit> DeleteGraphCommand { get; }
}
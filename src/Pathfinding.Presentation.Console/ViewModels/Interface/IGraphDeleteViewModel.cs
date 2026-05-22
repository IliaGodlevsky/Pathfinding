using ReactiveUI;
using System.Reactive;

namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface IGraphDeleteViewModel
{
    ReactiveCommand<Unit, Unit> DeleteGraphCommand { get; }
}
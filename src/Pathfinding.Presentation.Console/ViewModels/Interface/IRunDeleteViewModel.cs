using ReactiveUI;
using System.Reactive;

namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface IRunDeleteViewModel
{
    ReactiveCommand<Unit, Unit> DeleteRunsCommand { get; }
}
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface IRunUpdateViewModel
{
    ReactiveCommand<Unit, Unit> UpdateRunsCommand { get; }
}
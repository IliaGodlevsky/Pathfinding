using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels.Interface;

internal interface IRunDeleteViewModel
{
    ReactiveCommand<Unit, Unit> DeleteRunsCommand { get; }
}
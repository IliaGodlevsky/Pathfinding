using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels.Interface
{
    internal interface IRunUpdateViewModel
    {
        ReactiveCommand<Unit, Unit> UpdateRunsCommand { get; }
    }
}
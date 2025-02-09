using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModel.Interface
{
    internal interface IRunDeleteViewModel
    {
        ReactiveCommand<Unit, Unit> DeleteRunsCommand { get; }
    }
}
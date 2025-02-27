using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels.Interface
{
    internal interface IRunCreateViewModel
    {
        ReactiveCommand<Unit, Unit> CreateRunCommand { get; }
    }
}
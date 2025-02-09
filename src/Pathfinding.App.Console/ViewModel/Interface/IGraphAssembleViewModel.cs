using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModel.Interface
{
    internal interface IGraphAssembleViewModel
    {
        ReactiveCommand<Unit, Unit> AssembleGraphCommand { get; }
    }
}
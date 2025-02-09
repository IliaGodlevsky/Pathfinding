using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModel.Interface
{
    internal interface IGraphDeleteViewModel
    {
        ReactiveCommand<Unit, Unit> DeleteGraphCommand { get; }
    }
}
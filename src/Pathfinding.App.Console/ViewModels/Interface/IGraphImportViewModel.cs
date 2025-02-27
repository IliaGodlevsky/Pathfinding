using Pathfinding.App.Console.Models;
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels.Interface
{
    internal interface IGraphImportViewModel
    {
        ReactiveCommand<Func<StreamModel>, Unit> ImportGraphCommand { get; }
    }
}
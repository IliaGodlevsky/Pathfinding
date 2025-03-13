using Pathfinding.App.Console.Models;
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels.Interface
{
    internal interface IGraphImportViewModel
    {
        IReadOnlyCollection<StreamFormat> StreamFormats { get; }

        ReactiveCommand<Func<StreamModel>, Unit> ImportGraphCommand { get; }
    }
}
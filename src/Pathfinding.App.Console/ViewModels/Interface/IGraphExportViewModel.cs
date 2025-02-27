using Pathfinding.App.Console.Models;
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels.Interface
{
    internal interface IGraphExportViewModel
    {
        ExportOptions Options { get; set; }

        ReactiveCommand<Func<StreamModel>, Unit> ExportGraphCommand { get; }
    }
}
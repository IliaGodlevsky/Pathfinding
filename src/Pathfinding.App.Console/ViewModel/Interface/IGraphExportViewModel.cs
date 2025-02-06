using Pathfinding.App.Console.Model;
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModel.Interface
{
    internal interface IGraphExportViewModel
    {
        ExportOptions Options { get; set; }

        ReactiveCommand<Func<StreamModel>, Unit> ExportGraphCommand { get; }
    }
}
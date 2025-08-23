using Pathfinding.App.Console.Models;
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.App.Console.ViewModels.Interface;

internal interface IGraphExportViewModel
{
    ExportOptions Option { get; set; }

    IReadOnlyList<ExportOptions> AllowedOptions { get; }

    IReadOnlyCollection<StreamFormat> StreamFormats { get; }

    ReactiveCommand<Func<StreamModel>, Unit> ExportGraphCommand { get; }
}
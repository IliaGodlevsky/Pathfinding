using Pathfinding.Presentation.Console.Models;
using ReactiveUI;
using System.Reactive;

namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface IGraphExportViewModel
{
    ExportOptions Option { get; set; }

    IReadOnlyList<ExportOptions> AvailableOptions { get; }

    IReadOnlyCollection<SerializationFormat> SerializationFormats { get; }

    ReactiveCommand<Func<StreamModel>, Unit> ExportGraphCommand { get; }
}
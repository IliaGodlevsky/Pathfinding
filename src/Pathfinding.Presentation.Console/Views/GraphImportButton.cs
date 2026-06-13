using Pathfinding.Presentation.Console.Extensions;
using Pathfinding.Presentation.Console.Models;
using Pathfinding.Presentation.Console.Resources;
using Pathfinding.Presentation.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class GraphImportButton : Button
{
    private readonly CompositeDisposable disposables = [];

    public GraphImportButton(IGraphImportViewModel viewModel)
    {
        Initialize();
        this.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Select(_ =>
            {
                var fileName = GetFileName(viewModel);
                return string.IsNullOrEmpty(fileName.Path) || fileName.Format == null
                    ? new Func<StreamModel>(() => StreamModel.Empty)
                    : () => new(File.OpenRead(fileName.Path), fileName.Format, fileName.NeedsCompress);
            })
            .InvokeCommand(viewModel, x => x.ImportGraphCommand)
            .DisposeWith(disposables);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }

    private static (string Path, SerializationFormat? Format, bool NeedsCompress) GetFileName(IGraphImportViewModel viewModel)
    {
        var formats = viewModel.SerializationFormats
            .ToDictionary(x => x.ToExtensionRepresentation());
        using var dialog = new OpenDialog(Resource.Import,
            Resource.ChooseFile, [.. formats.Keys]);
        dialog.Width = Dim.Percent(45);
        dialog.Height = Dim.Percent(55);
        Application.Run(dialog);
        string filePath = dialog.FilePath.ToString();
        string extension = Path.GetExtension(filePath);
        bool needsDecompress = filePath.Contains(".gz.");
        return !dialog.Canceled
               && !string.IsNullOrEmpty(filePath)
               && formats.TryGetValue(extension, out var format)
            ? (filePath, format, needsDecompress)
            : (string.Empty, null, false);
    }
}

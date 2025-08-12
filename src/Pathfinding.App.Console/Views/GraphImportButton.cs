using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.Resources;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

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
                    : () => new(File.OpenRead(fileName.Path), fileName.Format);
            })
            .InvokeCommand(viewModel, x => x.ImportGraphCommand)
            .DisposeWith(disposables);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }

    private static (string Path, StreamFormat? Format) GetFileName(IGraphImportViewModel viewModel)
    {
        var formats = viewModel.StreamFormats
            .ToDictionary(x => x.ToExtensionRepresentation());
        using var dialog = new OpenDialog(Resource.Import,
            Resource.ChooseFile, [.. formats.Keys]);
        dialog.Width = Dim.Percent(45);
        dialog.Height = Dim.Percent(55);
        Application.Run(dialog);
        var filePath = dialog.FilePath.ToString();
        var extension = Path.GetExtension(filePath);
        return !dialog.Canceled
               && !string.IsNullOrEmpty(filePath)
               && formats.TryGetValue(extension, out var format)
            ? (filePath, format)
            : (string.Empty, null);
    }
}

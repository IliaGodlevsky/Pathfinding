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

internal sealed partial class GraphExportButton
{
    private readonly CompositeDisposable disposables = [];

    public GraphExportButton(IGraphExportViewModel viewModel)
    {
        Initialize();
        this.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Select(_ =>
            {
                var filePath = GetFilePath(viewModel);
                return string.IsNullOrEmpty(filePath.Path) || filePath.Format == null
                    ? new Func<StreamModel>(() => StreamModel.Empty)
                    : () => new(OpenWrite(filePath.Path), filePath.Format);
            })
            .InvokeCommand(viewModel, x => x.ExportGraphCommand)
            .DisposeWith(disposables);
        viewModel.ExportGraphCommand.CanExecute
            .BindTo(this, x => x.Enabled)
            .DisposeWith(disposables);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }

    private static FileStream OpenWrite(string path)
    {
        return new(path, FileMode.Create, FileAccess.Write, FileShare.None);
    }

    private static (string Path, SerializationFormat? Format, bool NeedsCompress) GetFilePath(IGraphExportViewModel viewModel)
    {
        var formats = viewModel.SerializationFormats
            .ToDictionary(x => x.ToExtensionRepresentation());
        using var dialog = new SaveDialog(Resource.Export,
            Resource.ChooseFile, [.. formats.Keys]);
        dialog.Width = Dim.Percent(45);
        dialog.Height = Dim.Percent(55);
        using var export = new GraphExportOptionsView(viewModel);
        export.ColorScheme = dialog.ColorScheme;
        export.Width = Dim.Percent(50);
        export.Height = 2;
        export.X = Pos.Center();
        export.Y = 5;
        dialog.Add(export);
        Application.Run(dialog);
        var filePath = dialog.FilePath.ToString();
        var extension = Path.GetExtension(filePath);
        var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        if (export.NeedsCompress)
        {
            filePath = string.Concat(filenameWithoutExtension, ".gz", extension);
        }
        return !dialog.Canceled
               && !string.IsNullOrEmpty(filePath)
               && formats.TryGetValue(extension, out var format)
            ? (filePath, format, export.NeedsCompress)
            : (string.Empty, null, false);
    }
}

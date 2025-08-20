using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.Resources;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphExportButton
{
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
                    : () => new (OpenWrite(filePath.Path), filePath.Format);
            })
            .InvokeCommand(viewModel, x => x.ExportGraphCommand);
        viewModel.ExportGraphCommand.CanExecute.BindTo(this, x => x.Enabled);
    }

    private static FileStream OpenWrite(string path)
    {
        return new(path, FileMode.Create, FileAccess.Write, FileShare.None);
    }

    private static (string Path, StreamFormat? Format) GetFilePath(IGraphExportViewModel viewModel)
    {
        var formats = viewModel.StreamFormats
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
        return !dialog.Canceled
               && !string.IsNullOrEmpty(filePath)
               && formats.TryGetValue(extension, out var format)
            ? (filePath, format)
            : (string.Empty, null);
    }
}

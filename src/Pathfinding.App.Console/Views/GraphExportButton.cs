using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.Resources;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class GraphExportButton
    {
        public GraphExportButton(IGraphExportViewModel viewModel)
        {
            Initialize();
            this.Events().MouseClick
                .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
                .Select(x => new Func<StreamModel>(() =>
                {
                    var filePath = GetFilePath(viewModel);
                    return string.IsNullOrEmpty(filePath.Path)
                        ? new(Stream.Null, null)
                        : new(OpenWrite(filePath.Path), filePath.Format);
                }))
                .InvokeCommand(viewModel, x => x.ExportGraphCommand);
            viewModel.ExportGraphCommand.CanExecute
                .BindTo(this, x => x.Enabled);
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
                Resource.ChooseFile, [.. formats.Keys])
            {
                Width = Dim.Percent(45),
                Height = Dim.Percent(55)
            };
            var export = new GraphExportOptionsView(viewModel)
            {
                ColorScheme = dialog.ColorScheme,
                Width = Dim.Percent(50),
                Height = 2,
                X = Pos.Center(),
                Y = 5
            };
            dialog.Add(export);
            Application.Run(dialog);
            string filePath = dialog.FilePath.ToString();
            string extension = Path.GetExtension(filePath);
            return !dialog.Canceled && dialog.FilePath != null
                ? (filePath, formats[extension])
                : (string.Empty, null);
        }
    }
}

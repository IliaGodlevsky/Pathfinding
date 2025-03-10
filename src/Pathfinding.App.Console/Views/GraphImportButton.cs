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
    internal sealed partial class GraphImportButton : Button
    {
        public GraphImportButton(IGraphImportViewModel viewModel)
        {
            Initialize();
            this.Events().MouseClick
                .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
                .Select(x => new Func<StreamModel>(() =>
                {
                    var fileName = GetFileName();
                    return string.IsNullOrEmpty(fileName.Path)
                        ? new(Stream.Null, null)
                        : new(File.OpenRead(fileName.Path), fileName.Format);
                }))
                .InvokeCommand(viewModel, x => x.ImportGraphCommand);
        }

        private static (string Path, StreamFormat? Format) GetFileName()
        {
            var formats = Enum.GetValues<StreamFormat>()
                .ToDictionary(x => x.ToExtensionRepresentation());
            using var dialog = new OpenDialog(Resource.Import,
                Resource.ChooseFile, [.. formats.Keys])
            {
                Width = Dim.Percent(45),
                Height = Dim.Percent(55)
            };
            Application.Run(dialog);
            string filePath = dialog.FilePath.ToString();
            string extension = Path.GetExtension(filePath);
            return !dialog.Canceled && dialog.FilePath != null
                ? (filePath, formats[extension])
                : (string.Empty, null);
        }
    }
}

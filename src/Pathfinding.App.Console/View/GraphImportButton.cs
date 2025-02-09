using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Model;
using Pathfinding.App.Console.ViewModel.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.View
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
            var extensions = formats.Keys.ToList();
            using var dialog = new OpenDialog("Import",
                "Choose file", extensions)
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

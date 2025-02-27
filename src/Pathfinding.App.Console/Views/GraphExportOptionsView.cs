using NStack;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed class GraphExportOptionsView : FrameView
    {
        private readonly CompositeDisposable disposables = [];
        private readonly RadioGroup exportOptions = new();

        public GraphExportOptionsView(IGraphExportViewModel viewModel)
        {
            var options = Enum.GetValues<ExportOptions>();
            var labels = options
                .Select(x => ustring.Make(x.ToStringRepresentation()))
                .ToArray();
            exportOptions.DisplayMode = DisplayModeLayout.Horizontal;
            exportOptions.RadioLabels = labels;
            Border = new Border();
            exportOptions.Events().SelectedItemChanged
                .Where(x => x.SelectedItem >= 0)
                .Select(x => options[x.SelectedItem])
                .BindTo(viewModel, x => x.Options)
                .DisposeWith(disposables);
            exportOptions.X = 1;
            exportOptions.Y = 1;
            exportOptions.SelectedItem = options.Length - 1;
            Add(exportOptions);
        }

        protected override void Dispose(bool disposing)
        {
            disposables.Dispose();
            base.Dispose(disposing);
        }
    }
}

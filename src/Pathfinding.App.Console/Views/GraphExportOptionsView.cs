using NStack;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed class GraphExportOptionsView : FrameView
{
    private readonly RadioGroup exportOptions = new();

    public DisplayModeLayout DisplayMode
    {
        get => exportOptions.DisplayMode;
        set => exportOptions.DisplayMode = value;
    }

    public GraphExportOptionsView(IGraphExportViewModel viewModel)
    {
        exportOptions.RadioLabels = [.. viewModel.AllowedOptions
            .Select(x => ustring.Make(x.ToStringRepresentation()))];
        DisplayMode = DisplayModeLayout.Horizontal;
        Border = new();
        exportOptions.Events().SelectedItemChanged
            .Where(x => x.SelectedItem >= 0)
            .Select(x => viewModel.AllowedOptions[x.SelectedItem])
            .BindTo(viewModel, x => x.Options);
        exportOptions.X = 1;
        exportOptions.Y = 1;
        exportOptions.SelectedItem = viewModel.AllowedOptions.Count - 1;
        Add(exportOptions);
    }
}

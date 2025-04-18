using Pathfinding.App.Console.ViewModels;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed class GraphUpdateButton : Button
{
    public GraphUpdateButton(GraphUpdateView view, GraphUpdateViewModel viewModel)
    {
        Text = "Update";
        viewModel.WhenAnyValue(x => x.SelectedGraphs)
            .Select(x => x.Length > 0)
            .BindTo(this, x => x.Enabled);
        this.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Do(x => view.Visible = true)
            .Subscribe();
    }
}

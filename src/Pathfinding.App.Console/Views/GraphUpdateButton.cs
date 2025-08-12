using Pathfinding.App.Console.ViewModels;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed class GraphUpdateButton : Button
{
    private readonly CompositeDisposable disposables = [];

    public GraphUpdateButton(GraphUpdateView view, GraphUpdateViewModel viewModel)
    {
        Text = "Update";
        viewModel.WhenAnyValue(x => x.SelectedGraphs)
            .Select(x => x.Length > 0)
            .BindTo(this, x => x.Enabled)
            .DisposeWith(disposables);
        this.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Do(x => view.Visible = true)
            .Subscribe()
            .DisposeWith(disposables);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}

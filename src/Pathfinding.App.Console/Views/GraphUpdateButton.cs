using Pathfinding.App.Console.ViewModels;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed class GraphUpdateButton : Button
{
    private readonly CompositeDisposable disposables = [];

    public GraphUpdateButton(GraphUpdateViewModel viewModel)
    {
        Text = "Update";
        viewModel.WhenAnyValue(x => x.SelectedGraphs)
            .Select(x => x.Length > 0)
            .BindTo(this, x => x.Enabled)
            .DisposeWith(disposables);
        this.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Subscribe(_ => ShowDialog(viewModel))
            .DisposeWith(disposables);
    }

    private static void ShowDialog(GraphUpdateViewModel viewModel)
    {
        var dialog = new GraphUpdateDialog(viewModel);
        Application.Run(dialog);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}

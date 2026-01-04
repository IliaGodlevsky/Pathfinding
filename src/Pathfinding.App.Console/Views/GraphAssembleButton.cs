using Pathfinding.App.Console.ViewModels;
using ReactiveMarbles.ObservableEvents;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphAssembleButton : Button
{
    private readonly CompositeDisposable disposables = [];

    public GraphAssembleButton(GraphAssembleViewModel viewModel)
    {
        Initialize();
        this.Events().MouseClick
            .Select(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Subscribe(_ => ShowDialog(viewModel))
            .DisposeWith(disposables);
    }

    private static void ShowDialog(GraphAssembleViewModel viewModel)
    {
        var dialog = new GraphAssembleDialog(viewModel);
        Application.Run(dialog);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}

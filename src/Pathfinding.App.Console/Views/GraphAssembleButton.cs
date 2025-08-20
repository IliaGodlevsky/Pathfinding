using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphAssembleButton : Button
{
    private readonly CompositeDisposable disposables = [];

    public GraphAssembleButton(GraphAssembleView view)
    {
        Initialize();
        this.Events().MouseClick
            .Select(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .BindTo(view, x => x.Visible)
            .DisposeWith(disposables);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}

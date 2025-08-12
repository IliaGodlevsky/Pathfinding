using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphDeleteButton : Button
{
    private readonly CompositeDisposable disposables = [];

    public GraphDeleteButton(IGraphDeleteViewModel viewModel)
    {
        Initialize();
        viewModel.DeleteGraphCommand.CanExecute
            .BindTo(this, x => x.Enabled)
            .DisposeWith(disposables);
        this.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Select(_ => Unit.Default)
            .InvokeCommand(viewModel, x => x.DeleteGraphCommand)
            .DisposeWith(disposables);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}

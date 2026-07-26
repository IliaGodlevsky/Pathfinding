using Pathfinding.Presentation.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

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
            .Where(_ => ConfirmDeletion())
            .Select(_ => Unit.Default)
            .InvokeCommand(viewModel, x => x.DeleteGraphCommand)
            .DisposeWith(disposables);
    }

    private static bool ConfirmDeletion()
    {
        return MessageBox.Query(
            "Delete graphs?",
            "Delete the selected graphs and all of their runs?\nThis action cannot be undone.",
            "Cancel",
            "Delete") == 1;
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}

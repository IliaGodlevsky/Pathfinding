using Pathfinding.Presentation.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class RunDeleteButton : Button
{
    private readonly CompositeDisposable disposables = [];

    public RunDeleteButton(IRunDeleteViewModel viewModel)
    {
        Initialize();
        viewModel.DeleteRunsCommand.CanExecute
            .BindTo(this, x => x.Enabled)
            .DisposeWith(disposables);
        this.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Where(_ => ConfirmDeletion())
            .Select(x => Unit.Default)
            .InvokeCommand(viewModel, x => x.DeleteRunsCommand)
            .DisposeWith(disposables);
    }

    private static bool ConfirmDeletion()
    {
        return MessageBox.Query(
            "Delete runs?",
            "Delete the selected runs?\nThis action cannot be undone.",
            "Cancel",
            "Delete") == 1;
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}

using Pathfinding.App.Console.Resources;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed class RunUpdateButton : Button
{
    private readonly CompositeDisposable disposables = [];

    public RunUpdateButton(IRunUpdateViewModel viewModel)
    {
        X = Pos.Percent(33);
        Y = 0;
        Width = Dim.Percent(33);
        Text = Resource.UpdateAlgorithms;
        viewModel.UpdateRunsCommand.CanExecute
            .BindTo(this, x => x.Enabled)
            .DisposeWith(disposables);
        this.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Throttle(TimeSpan.FromMilliseconds(30))
            .Select(x => Unit.Default)
            .InvokeCommand(viewModel, x => x.UpdateRunsCommand)
            .DisposeWith(disposables);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}
